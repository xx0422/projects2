#include "plotter_scene.h"

#include <math.h>
#include <stdio.h>

/* ===== Worker param szálakhoz ===== */

struct WorkerParams {
    int worker_index;
    int worker_count;

    int    sample_count;
    double x_min;
    double step;

    int         func_count;
    const char (*exprs)[256];
    const int  *enabled;
    double     (*values)[MAX_SAMPLES];
};

static void worker_thread_func(void *userdata) {
    WorkerParams *p = (WorkerParams *)userdata;

    int start = (p->sample_count * p->worker_index) / p->worker_count;
    int end   = (p->sample_count * (p->worker_index + 1)) / p->worker_count;

    int i, f;
    for (i = start; i < end; ++i) {
        double x = p->x_min + p->step * (double)i;
        for (f = 0; f < p->func_count; ++f) {
            if (!p->enabled[f]) {
                p->values[f][i] = 0.0;
                continue;
            }

            const char *expr = p->exprs[f];
            if (!expr || expr[0] == '\0') {
                p->values[f][i] = 0.0;
                continue;
            }

            int ok = 1;
            double y = eval_expression(expr, x, &ok);
            if (!ok) {
                p->values[f][i] = 0.0;
            } else {
                p->values[f][i] = y;
            }
        }
    }
}

/* ===== Konstruktor / Destruktor ===== */

PlotterScene::PlotterScene() {
    x_min = -10.0;
    x_max =  10.0;
    step  =   0.1;
    thread_count = 4;

    int i;
    for (i = 0; i < MAX_FUNCS; ++i) {
        exprs[i][0] = '\0';
        enabled[i] = (i < 2) ? 1 : 0; // elsõ kettõ alapból bekapcsolva

        // alap színek: piros, zöld, kék, sárgás
        if (i == 0) { colors[i][0] = 1.0f; colors[i][1] = 0.0f; colors[i][2] = 0.0f; }
        else if (i == 1) { colors[i][0] = 0.0f; colors[i][1] = 1.0f; colors[i][2] = 0.0f; }
        else if (i == 2) { colors[i][0] = 0.0f; colors[i][1] = 0.0f; colors[i][2] = 1.0f; }
        else { colors[i][0] = 1.0f; colors[i][1] = 1.0f; colors[i][2] = 0.0f; }
    }

    // default képletek
    const char *def0 = "sin(x)";
    const char *def1 = "cos(x)";
    int j;
    for (j = 0; def0[j] != '\0' && j < 255; ++j) exprs[0][j] = def0[j];
    exprs[0][j] = '\0';
    for (j = 0; def1[j] != '\0' && j < 255; ++j) exprs[1][j] = def1[j];
    exprs[1][j] = '\0';

    sample_count = 0;
    has_results = 0;
    y_min = -1.0;
    y_max =  1.0;
}

PlotterScene::~PlotterScene() {
}

/* ===== Scene interface ===== */

void PlotterScene::input_event(const Ref<InputEvent> &event) {
    // most semmit nem kezelünk itt
    (void)event;
}

void PlotterScene::update(float delta) {
    // logika nincs per-frame, minden a gombra kötve
    (void)delta;
}

/* ===== Számítás több szálon ===== */

void PlotterScene::compute_samples() {
    if (step <= 0.0) {
        has_results = 0;
        return;
    }
    if (x_max <= x_min) {
        has_results = 0;
        return;
    }

    double range = x_max - x_min;
    int n = (int)(range / step) + 1;
    if (n < 2) n = 2;
    if (n > MAX_SAMPLES) n = MAX_SAMPLES;
    sample_count = n;

    if (thread_count < 1) thread_count = 1;
    if (thread_count > 32) thread_count = 32;
    if (thread_count > sample_count) thread_count = sample_count;

    Thread threads[32];
    WorkerParams params[32];

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

    for (i = 0; i < thread_count; ++i) {
        threads[i].wait_to_finish();
    }

    compute_y_minmax();
    has_results = 1;
}

void PlotterScene::compute_y_minmax() {
    int first = 1;
    double ymin = 0.0;
    double ymax = 0.0;

    int f, i;
    for (f = 0; f < MAX_FUNCS; ++f) {
        if (!enabled[f]) continue;
        if (exprs[f][0] == '\0') continue;

        for (i = 0; i < sample_count; ++i) {
            double y = values[f][i];
            if (first) {
                ymin = ymax = y;
                first = 0;
            } else {
                if (y < ymin) ymin = y;
                if (y > ymax) ymax = y;
            }
        }
    }

    if (first) {
        ymin = -1.0;
        ymax =  1.0;
    } else {
        if (ymax == ymin) {
            ymax = ymin + 1.0;
        }
    }

    y_min = ymin;
    y_max = ymax;
}

/* ===== Rajzolás: tengelyek + görbék ===== */

void PlotterScene::draw_axes_and_curves() {
    Renderer *r = Renderer::get_singleton();
    Vector2i win_size = r->get_window_size();

    float left   = 60.0f;
    float right  = (float)win_size.x - 20.0f;
    float top    = 40.0f;
    float bottom = (float)win_size.y - 60.0f;

    if (right <= left + 10.0f) return;
    if (bottom <= top + 10.0f) return;

    float width  = right - left;
    float height = bottom - top;

    double xmin = x_min;
    double xmax = x_max;
    double ymin = y_min;
    double ymax = y_max;

    if (xmax == xmin) xmax = xmin + 1.0;
    if (ymax == ymin) ymax = ymin + 1.0;

    Color axis_color(1.0f, 1.0f, 1.0f);
    Color grid_color(0.3f, 0.3f, 0.3f);

    // x tengely (y=0) ha beleesik
    if (ymin <= 0.0 && ymax >= 0.0) {
        double t = (0.0 - ymin) / (ymax - ymin);
        float y = (float)(bottom - t * height);
        r->draw_line(Vector2(left, y), Vector2(right, y), axis_color, 2.0f);
    }

    // y tengely (x=0) ha beleesik
    if (xmin <= 0.0 && xmax >= 0.0) {
        double t = (0.0 - xmin) / (xmax - xmin);
        float x = (float)(left + t * width);
        r->draw_line(Vector2(x, top), Vector2(x, bottom), axis_color, 2.0f);
    }

    // keret
    Rect2 border_rect(Vector2(left, top), Vector2(width, height));
    r->draw_line_rect(border_rect, grid_color, 1.0f);

    if (!has_results) return;

    int f;
    for (f = 0; f < MAX_FUNCS; ++f) {
        if (!enabled[f]) continue;
        if (exprs[f][0] == '\0') continue;

        Color col(colors[f][0], colors[f][1], colors[f][2]);

        int i;
        for (i = 1; i < sample_count; ++i) {
            double x0 = xmin + step * (double)(i - 1);
            double x1 = xmin + step * (double)i;
            double y0 = values[f][i - 1];
            double y1 = values[f][i];

            double tx0 = (x0 - xmin) / (xmax - xmin);
            double tx1 = (x1 - xmin) / (xmax - xmin);
            double ty0 = (y0 - ymin) / (ymax - ymin);
            double ty1 = (y1 - ymin) / (ymax - ymin);

            float sx0 = (float)(left + tx0 * width);
            float sx1 = (float)(left + tx1 * width);
            float sy0 = (float)(bottom - ty0 * height);
            float sy1 = (float)(bottom - ty1 * height);

            r->draw_line(Vector2(sx0, sy0), Vector2(sx1, sy1), col, 2.0f);
        }
    }
}

/* ===== Kirajzolás + GUI ===== */

void PlotterScene::render() {
    Renderer *r = Renderer::get_singleton();

    r->clear_screen(Color(0.05f, 0.05f, 0.08f, 1.0f));
    r->camera_2d_projection_set_to_window();

    // grafikon
    draw_axes_and_curves();

    // GUI
    GUI::new_frame();

    ImGui::Begin("Multithreadelt Plotter");

    ImGui::Text("Fuggveny: f(x), placeholder: x");
    ImGui::Separator();

    ImGui::InputDouble("X min", &x_min);
    ImGui::InputDouble("X max", &x_max);
    ImGui::InputDouble("Lepes (step)", &step);

    ImGui::SliderInt("Szalak szama", &thread_count, 1, 32);

    ImGui::Separator();
    ImGui::Text("Fuggvenyek (max 4):");

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

    if (ImGui::Button("Szamolas + Kirajzolas")) {
        compute_samples();
    }

    ImGui::Separator();
    ImGui::Text("Aktualis tartomany:");
    ImGui::Text("X: [%.3f , %.3f]", x_min, x_max);
    ImGui::Text("Y: [%.3f , %.3f]", y_min, y_max);

    ImGui::End();

    GUI::render();
}
