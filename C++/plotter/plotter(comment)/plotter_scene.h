#ifndef PLOTTER_SCENE_H
#define PLOTTER_SCENE_H

#include "sfw.h"
#include "expression.h"

#define MAX_FUNCS   4
#define MAX_SAMPLES 4096

class PlotterScene : public Scene {
    SFW_OBJECT(PlotterScene, Scene);

public:
    PlotterScene();
    virtual ~PlotterScene();

    virtual void input_event(const Ref<InputEvent> &event);
    virtual void update(float delta);
    virtual void render();

private:
    // GUI/állapot
    double x_min;
    double x_max;
    double step;
    int    thread_count;

    char  exprs[MAX_FUNCS][256];
    int   enabled[MAX_FUNCS];     // 0 vagy 1
    float colors[MAX_FUNCS][3];   // RGB 0..1

    // számolt adatok
    double values[MAX_FUNCS][MAX_SAMPLES];
    int    sample_count;
    int    has_results;

    double y_min;
    double y_max;

    // szálas számítás
    void compute_samples();
    void compute_y_minmax();

    // kirajzolás
    void draw_axes_and_curves();
};

#endif // PLOTTER_SCENE_H
