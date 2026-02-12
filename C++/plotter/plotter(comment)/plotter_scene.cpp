#include "plotter_scene.h"

#include <math.h>
#include <stdio.h>

/* ============================================================
   =============== WORKER PARAMÉTEREK SZÁLAKHOZ ===============
   ============================================================
   Ez a struktúra tartalmazza, hogy egy adott worker szálnak
   pontosan melyik mintaindextől meddig kell számolnia.
   Minden szál megkapja:

   - saját indexét
   - összes szál számát
   - x tartomány adatait (x_min, step)
   - összes függvény képletét és engedélyezését
   - a tömböt, ahová az eredményeket írni kell
   ============================================================ */

struct WorkerParams {
    int worker_index;   // aktuális szál indexe
    int worker_count;   // hány szál összesen

    int    sample_count; // összes számítandó mintapont
    double x_min;        // X tartomány minimuma
    double step;         // lépésköz (step)

    int         func_count;               // függvények száma
    const char (*exprs)[256];             // a függvényképletek tömbje
    const int  *enabled;                  // melyik függvény engedélyezett
    double     (*values)[MAX_SAMPLES];    // eredményeket tároló mátrix
};

/* ============================================================
   =============== WORKER SZÁL FÜGGVÉNY =======================
   ============================================================
   Ez a függvény fut minden szálon.

   A szál a neki jutó [start..end) tartományban
   kiszámolja az összes engedélyezett függvény y értékeit.
   ============================================================ */

static void worker_thread_func(void *userdata) {
    WorkerParams *p = (WorkerParams *)userdata;

    // A szál a mintapontokat egyenlően felosztva kapja
    int start = (p->sample_count * p->worker_index) / p->worker_count;
    int end   = (p->sample_count * (p->worker_index + 1)) / p->worker_count;

    // i = mintapont index
    // f = aktuális függvény
    int i, f;
    for (i = start; i < end; ++i) {

        // kiszámoljuk az x-et
        double x = p->x_min + p->step * (double)i;

        // végigmegyünk minden függvényen
        for (f = 0; f < p->func_count; ++f) {

            // ha nem engedélyezett, akkor 0-t írunk és megyünk tovább
            if (!p->enabled[f]) {
                p->values[f][i] = 0.0;
                continue;
            }

            // ha nincs megadva képlet → 0
            const char *expr = p->exprs[f];
            if (!expr || expr[0] == '\0') {
                p->values[f][i] = 0.0;
                continue;
            }

            // kiértékeljük a képletet (math parser)
            int ok = 1;
            double y = eval_expression(expr, x, &ok);

            // érvénytelen érték esetén 0-t írunk
            p->values[f][i] = ok ? y : 0.0;
        }
    }
}

/* ============================================================
   ================== KONSTRUKTOR / DESTRUKTOR =================
   ============================================================ */

PlotterScene::PlotterScene() {
    // alap tartomány
    x_min = -10.0;
    x_max =  10.0;
    step  =   0.1;

    // alap szál szám
    thread_count = 4;

    // változók inicializálása
    int i;
    for (i = 0; i < MAX_FUNCS; ++i) {
        exprs[i][0] = '\0';                      // üres képlet
        enabled[i] = (i < 2) ? 1 : 0;            // első két függvény alapból bekapcsolva

        // alapértelmezett színek
        if (i == 0) { colors[i][0] = 1.0f; colors[i][1] = 0.0f; colors[i][2] = 0.0f; }        // piros
        else if (i == 1) { colors[i][0] = 0.0f; colors[i][1] = 1.0f; colors[i][2] = 0.0f; }   // zöld
        else if (i == 2) { colors[i][0] = 0.0f; colors[i][1] = 0.0f; colors[i][2] = 1.0f; }   // kék
        else { colors[i][0] = 1.0f; colors[i][1] = 1.0f; colors[i][2] = 0.0f; }               // sárga
    }

    // két alap képlet
    const char *def0 = "sin(x)";
    const char *def1 = "cos(x)";

    // bemásoljuk őket
    int j;
    for (j = 0; def0[j] != '\0' && j < 255; ++j) exprs[0][j] = def0[j];
    exprs[0][j] = '\0';
    for (j = 0; def1[j] != '\0' && j < 255; ++j) exprs[1][j] = def1[j];
    exprs[1][j] = '\0';

    sample_count = 0;   // még nincs számítva
    has_results  = 0;
    y_min = -1.0;       // alap Y tartomány
    y_max =  1.0;
}

PlotterScene::~PlotterScene() {
    // nincs dinamikus memória, nincs teendő
}

/* ============================================================
   ============= GODOT / RENDER ENGINE HOOKOK =================
   ============================================================ */

void PlotterScene::input_event(const Ref<InputEvent> &event) {
    // egyelőre nem használjuk
    (void)event;
}

void PlotterScene::update(float delta) {
    // nincs per-frame logika
    (void)delta;
}

/* ============================================================
   ============== FÜGGVÉNYEK KISZÁMOLÁSA TÖBB SZÁLON ===========
   ============================================================ */

void PlotterScene::compute_samples() {

    // hibás beállítás esetén abort
    if (step <= 0.0 || x_max <= x_min) {
        has_results = 0;
        return;
    }

    // a szükséges mintapontok száma
    double range = x_max - x_min;
    int n = (int)(range / step) + 1;

    if (n < 2) n = 2;
    if (n > MAX_SAMPLES) n = MAX_SAMPLES;

    sample_count = n;

    // a szálak számát normalizáljuk
    if (thread_count < 1) thread_count = 1;
    if (thread_count > 32) thread_count = 32;
    if (thread_count > sample_count) thread_count = sample_count;

    Thread threads[32];
    WorkerParams params[32];

    // minden szálnak átadjuk a munkát
    int i;
    for (i = 0; i < thread_count; ++i) {
        params[i].worker_index = i;
        params[i].worker_count = thread_count;
        params[i].sample_count = sample_count;
        params[i].x_min = x_min;
        params[i].step  = step;
        params[i].func_count = MAX_FUNCS;
        params[i].exprs   = exprs;
        params[i].enabled = enabled;
        params[i].values  = values;

        threads[i].start(worker_thread_func, &params[i]);
    }

    // megvárjuk az összes szál befejezését
    for (i = 0; i < thread_count; ++i) {
        threads[i].wait_to_finish();
    }

    // global y_min / y_max kiszámítása
    compute_y_minmax();
    has_results = 1;
}

/* ============================================================
   ============= Y MIN / MAX AUTOMATIKUS KISZÁMÍTÁSA ===========
   ============================================================ */

void PlotterScene::compute_y_minmax() {

    int first = 1;
    double ymin = 0.0;
    double ymax = 0.0;

    // minden engedélyezett függvényen végigmegyünk
    int f, i;
    for (f = 0; f < MAX_FUNCS; ++f) {
        if (!enabled[f]) continue;
        if (exprs[f][0] == '\0') continue;

        for (i = 0; i < sample_count; ++i) {
            double y = values[f][i];

            if (first) {
            // első talált érték
                ymin = ymax = y;
                first = 0;
            } else {
                if (y < ymin) ymin = y;
                if (y > ymax) ymax = y;
            }
        }
    }

    // ha semmi nincs engedélyezve → alap skála
    if (first) {
        ymin = -1.0;
        ymax =  1.0;
    }
    // ha minden érték azonos
    else if (ymax == ymin) {
        ymax = ymin + 1.0;
    }

    y_min = ymin;
    y_max = ymax;
}

/* ============================================================
   ===================== GRAFIKON KIRAJZOLÁSA ==================
   ============================================================ */

void PlotterScene::draw_axes_and_curves() {
    Renderer *r = Renderer::get_singleton();
    Vector2i win_size = r->get_window_size();

    // grafikon margók
    float left   = 60.0f;
    float right  = (float)win_size.x - 20.0f;
    float top    = 40.0f;
    float bottom = (float)win_size.y - 60.0f;

    if (right <= left + 10.0f || bottom <= top + 10.0f)
        return;

    float width  = right - left;
    float height = bottom - top;

    // x és y megjelenítési tartomány
    double xmin = x_min;
    double xmax = x_max;
    double ymin = y_min;
    double ymax = y_max;

    // degenerált esetek kezelése
    if (xmax == xmin) xmax = xmin + 1.0;
    if (ymax == ymin) ymax = ymin + 1.0;

    Color axis_color(1.0f, 1.0f, 1.0f);    // tengely színe
    Color grid_color(0.3f, 0.3f, 0.3f);    // keret színe

    // X tengely (y=0)
    if (ymin <= 0.0 && ymax >= 0.0) {
        double t = (0.0 - ymin) / (ymax - ymin);
        float y = (float)(bottom - t * height);
        r->draw_line(Vector2(left, y), Vector2(right, y), axis_color, 2.0f);
    }

    // Y tengely (x=0)
    if (xmin <= 0.0 && xmax >= 0.0) {
        double t = (0.0 - xmin) / (xmax - xmin);
        float x = (float)(left + t * width);
        r->draw_line(Vector2(x, top), Vector2(x, bottom), axis_color, 2.0f);
    }

    // keret
    Rect2 border_rect(Vector2(left, top), Vector2(width, height));
    r->draw_line_rect(border_rect, grid_color, 1.0f);

    // ha még nincsenek kész eredmények, nincs mit rajzolni
    if (!has_results) return;

    // itt rajzoljuk ki az összes függvényt
    int f;
    for (f = 0; f < MAX_FUNCS; ++f) {
        if (!enabled[f] || exprs[f][0] == '\0')
            continue;

        Color col(colors[f][0], colors[f][1], colors[f][2]);   // a függvény saját színe

        int i;
        for (i = 1; i < sample_count; ++i) {

            // mintapontok x és y értékei
            double x0 = xmin + step * (double)(i - 1);
            double x1 = xmin + step * (double)i;
            double y0 = values[f][i - 1];
            double y1 = values[f][i];

            // normalizálás 0..1 tartományra
            double tx0 = (x0 - xmin) / (xmax - xmin);
            double tx1 = (x1 - xmin) / (xmax - xmin);
            double ty0 = (y0 - ymin) / (ymax - ymin);
            double ty1 = (y1 - ymin) / (ymax - ymin);

            // képernyőre vetítés
            float sx0 = (float)(left + tx0 * width);
            float sx1 = (float)(left + tx1 * width);
            float sy0 = (float)(bottom - ty0 * height);
            float sy1 = (float)(bottom - ty1 * height);

            // függvény vonal kirajzolása
            r->draw_line(Vector2(sx0, sy0), Vector2(sx1, sy1), col, 2.0f);
        }
    }
}

/* ============================================================
   ======================= RENDER + GUI ========================
   ============================================================ */

void PlotterScene::render() {
    Renderer *r = Renderer::get_singleton();

    // háttér
    r->clear_screen(Color(0.05f, 0.05f, 0.08f, 1.0f));
    r->camera_2d_projection_set_to_window();

    // grafikon kirajzolása
    draw_axes_and_curves();

    // GUI indítása
    GUI::new_frame();

    ImGui::Begin("Multithreadelt Plotter");

    ImGui::Text("Fuggveny: f(x), placeholder: x");
    ImGui::Separator();

    // x tartomány + step
    ImGui::InputDouble("X min", &x_min);
    ImGui::InputDouble("X max", &x_max);
    ImGui::InputDouble("Lepes (step)", &step);

    // szálak száma
    ImGui::SliderInt("Szalak szama", &thread_count, 1, 32);

    ImGui::Separator();
    ImGui::Text("Fuggvenyek (max 4):");

    // függvény képletek + engedélyezés + szín
    int i;
    for (i = 0; i < MAX_FUNCS; ++i) {
        char label[64];

        sprintf(label, "f%d(x)", i + 1);
        ImGui::InputText(label, exprs[i], 256);

        sprintf(label, "F%d engedelyezese", i + 1);
        ImGui::Checkbox(label, (bool *)&enabled[i]);

        sprintf(label, "Szin f%d", i + 1);
        ImGui::ColorEdit3(label, colors[i]);

        ImGui::Separator();
    }

    // gomb a számításhoz
    if (ImGui::Button("Szamolas + Kirajzolas")) {
        compute_samples();
    }

    // aktuális Y tartomány kiírása
    ImGui::Separator();
    ImGui::Text("Aktualis tartomany:");
    ImGui::Text("X: [%.3f , %.3f]", x_min, x_max);
    ImGui::Text("Y: [%.3f , %.3f]", y_min, y_max);

    ImGui::End();

    // GUI kirenderelése
    GUI::render();
}
