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
    const char *s;
    double x_value;
    int error;
} Parser;

/* --- segédfüggvények --- */

static void skip_spaces(Parser *p) {
    while (*p->s == ' ' || *p->s == '\t' || *p->s == '\n' || *p->s == '\r')
        p->s++;
}

static int match(Parser *p, const char *kw) {
    skip_spaces(p);
    const char *a = p->s;
    const char *b = kw;
    while (*b && *a == *b) {
        a++; b++;
    }
    if (*b == '\0') {
        p->s = a;
        return 1;
    }
    return 0;
}

static int match_char(Parser *p, char c) {
    skip_spaces(p);
    if (*p->s == c) {
        p->s++;
        return 1;
    }
    return 0;
}

/* Előre kell deklarálni */
static double parse_expr(Parser *p);

/* --- szám, x, függvények --- */
static double parse_number_or_func(Parser *p) {
    skip_spaces(p);

    /* x változó */
    if (*p->s == 'x' || *p->s == 'X') {
        p->s++;
        return p->x_value;
    }

    /* sin(...) */
    if (match(p, "sin")) {
        if (!match_char(p, '(')) { p->error = 1; return 0; }
        double v = parse_expr(p);
        if (!match_char(p, ')')) { p->error = 1; }
        return sin(v);
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
        if (v < 0) { p->error = 1; return 0; }
        return sqrt(v);
    }

    /* pow(a,b) */
    if (match(p, "pow")) {
        if (!match_char(p, '(')) { p->error = 1; return 0; }

        double base = parse_expr(p);

        if (!match_char(p, ',')) { p->error = 1; return 0; }

        double expo = parse_expr(p);

        if (!match_char(p, ')')) { p->error = 1; }

        return pow(base, expo);
    }

    /* szám olvasása: [+/-]digits[.digits] */
    const char *start = p->s;
    int has_digit = 0;

    if (*p->s == '+' || *p->s == '-')
        p->s++;

    while (isdigit(*p->s)) {
        p->s++;
        has_digit = 1;
    }

    if (*p->s == '.') {
        p->s++;
        while (isdigit(*p->s)) {
            p->s++;
            has_digit = 1;
        }
    }

    if (!has_digit) {
        p->error = 1;
        return 0.0;
    }

    /* konvertálás */
    double val = atof(start);
    return val;
}

/* --- faktor: szám, zárójel, +/− prefix --- */
static double parse_factor(Parser *p) {
    skip_spaces(p);

    /* +prefix */
    if (match_char(p, '+')) {
        return parse_factor(p);
    }

    /* -prefix */
    if (match_char(p, '-')) {
        return -parse_factor(p);
    }

    /* (expr) */
    if (match_char(p, '(')) {
        double v = parse_expr(p);
        if (!match_char(p, ')')) {
            p->error = 1;
        }
        return v;
    }

    return parse_number_or_func(p);
}

/* --- term: * és / --- */
static double parse_term(Parser *p) {
    double v = parse_factor(p);

    while (!p->error) {
        if (match_char(p, '*')) {
            v *= parse_factor(p);
        } else if (match_char(p, '/')) {
            double d = parse_factor(p);
            if (d == 0.0) {
                p->error = 1;
                return 0;
            }
            v /= d;
        } else {
            break;
        }
    }

    return v;
}

/* --- expr: + és - --- */
static double parse_expr(Parser *p) {
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
    p.s = expr;
    p.x_value = x;
    p.error = 0;

    double v = parse_expr(&p);

    skip_spaces(&p);
    if (*p.s != '\0') {
        p.error = 1;
    }

    if (ok) *ok = (p.error == 0);
    return p.error ? 0.0 : v;
}
