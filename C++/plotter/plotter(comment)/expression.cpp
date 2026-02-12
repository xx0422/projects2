#include "expression.h"
#include <math.h>
#include <ctype.h>

/* ------------------------------
   Egyszerű rekurzív leszálló parser
   Követelmények:
    +  -  *  /
    sin(x)
    cos(x)
    sqrt(x)
    pow(a,b)
    zárójelek
    x változó
-------------------------------- */

/* Parser állapot */
typedef struct {
    const char *s;     // az aktuális pozíció a bemeneti sztringben
    double x_value;    // x változó értéke
    int error;         // hibajelző flag
} Parser;

/* --- segédfüggvények --- */

static void skip_spaces(Parser *p) {
    // kihagyja az üres helyeket, hogy ne zavarják a parsolást
    while (*p->s == ' ' || *p->s == '\t' || *p->s == '\n' || *p->s == '\r')
        p->s++;
}

static int match(Parser *p, const char *kw) {
    // próbálja ellenőrizni, hogy a bemenet elején ott van-e egy kulcsszó (pl. "sin")
    skip_spaces(p);
    const char *a = p->s;
    const char *b = kw;
    while (*b && *a == *b) {
        a++; b++;        // karakterenként összehasonlít
    }
    if (*b == '\0') {    // ha a kulcsszó véget ért, akkor sikeres
        p->s = a;        // előrelép a bemenetben
        return 1;
    }
    return 0;            // különben nem egyezik
}

static int match_char(Parser *p, char c) {
    // egyetlen karakter ellenőrzése (pl. zárójel, +, -, *, /)
    skip_spaces(p);
    if (*p->s == c) {
        p->s++;          // karakter elfogyasztása
        return 1;
    }
    return 0;
}

/* Előre kell deklarálni, mert a függvények rekurzívan hívják egymást */
static double parse_expr(Parser *p);

/* --- szám, x, függvények --- */
static double parse_number_or_func(Parser *p) {
    skip_spaces(p);

    /* x változó */
    if (*p->s == 'x' || *p->s == 'X') {
        p->s++;                // x elfogyasztása
        return p->x_value;     // x helyére behelyettesítjük az értéket
    }

    /* sin(...) */
    if (match(p, "sin")) {
        if (!match_char(p, '(')) { p->error = 1; return 0; }
        double v = parse_expr(p);   // argumentum parsolása
        if (!match_char(p, ')')) { p->error = 1; }
        return sin(v);              // függvény alkalmazása
    }

    /* cos(...) */
    if (match(p, "cos")) {
        if (!match_char(p, '(')) { p->error = 1; return 0; }
        double v = parse_expr(p);
        if (!match_char(p, ')')) { p->error = 1; }
        return cos(v);
    }

    /* sqrt(...) */
    if (match(p, "sqrt")) {
        if (!match_char(p, '(')) { p->error = 1; return 0; }
        double v = parse_expr(p);
        if (!match_char(p, ')')) { p->error = 1; }
        if (v < 0) { p->error = 1; return 0; }  // sqrt negatívra invalid
        return sqrt(v);
    }

    /* pow(a,b) */
    if (match(p, "pow")) {
        if (!match_char(p, '(')) { p->error = 1; return 0; }

        double base = parse_expr(p);   // az első argumentum

        if (!match_char(p, ',')) { p->error = 1; return 0; }

        double expo = parse_expr(p);   // második argumentum

        if (!match_char(p, ')')) { p->error = 1; }

        return pow(base, expo);        // hatványzás
    }

    /* szám olvasása: [+/-]digits[.digits] */
    const char *start = p->s;
    int has_digit = 0;

    if (*p->s == '+' || *p->s == '-')
        p->s++;            // előjel engedélyezése

    while (isdigit(*p->s)) {
        p->s++;
        has_digit = 1;     // legalább egy számjegy kell
    }

    if (*p->s == '.') {    // tizedespont kezelése
        p->s++;
        while (isdigit(*p->s)) {
            p->s++;
            has_digit = 1;
        }
    }

    if (!has_digit) {      // ha nincs számjegy → hiba
        p->error = 1;
        return 0.0;
    }

    /* konvertálás */
    double val = atof(start);  // a stringet lebegőpontos számmá alakítja
    return val;
}

/* --- faktor: szám, zárójel, +/− prefix --- */
static double parse_factor(Parser *p) {
    skip_spaces(p);

    /* +prefix (nincs hatása, csak átugorja) */
    if (match_char(p, '+')) {
        return parse_factor(p);
    }

    /* -prefix (előjelváltás) */
    if (match_char(p, '-')) {
        return -parse_factor(p);
    }

    /* (expr) zárójeles kifejezés */
    if (match_char(p, '(')) {
        double v = parse_expr(p);
        if (!match_char(p, ')')) {
            p->error = 1;     // ha nincs lezáró zárójel → hiba
        }
        return v;
    }

    return parse_number_or_func(p);   // szám vagy függvény hívása
}

/* --- term: * és / --- */
static double parse_term(Parser *p) {
    // első faktor beolvasása
    double v = parse_factor(p);

    // amíg * vagy / jön, sorozatban alkalmazza őket
    while (!p->error) {
        if (match_char(p, '*')) {
            v *= parse_factor(p);
        } else if (match_char(p, '/')) {
            double d = parse_factor(p);
            if (d == 0.0) {
                p->error = 1;       // 0-val osztás hiba
                return 0;
            }
            v /= d;
        } else {
            break;                  // nincs több művelet
        }
    }

    return v;
}

/* --- expr: + és - --- */
static double parse_expr(Parser *p) {
    // term olvasása (+,- legalacsonyabb precedencia)
    double v = parse_term(p);

    while (!p->error) {
        if (match_char(p, '+')) {
            v += parse_term(p);
        } else if (match_char(p, '-')) {
            v -= parse_term(p);
        } else {
            break;
        }
    }

    return v;
}

/* --- külső függvény --- */
double eval_expression(const char *expr, double x, int *ok) {
    Parser p;
    p.s = expr;       // feldolgozandó kifejezés
    p.x_value = x;    // x változó értéke
    p.error = 0;      // kezdetben nincs hiba

    double v = parse_expr(&p);

    skip_spaces(&p);
    if (*p.s != '\0') {
        p.error = 1;  // ha maradt feldolgozatlan karakter → hiba
    }

    if (ok) *ok = (p.error == 0);
    return p.error ? 0.0 : v;   // hiba esetén 0-t ad vissza
}
