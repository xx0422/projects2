#include "plotter_application.h"

PlotterApplication::PlotterApplication() {
    Renderer::initialize();
    GUI::initialize();
    scene = Ref<Scene>(memnew(PlotterScene));
}

PlotterApplication::~PlotterApplication() {
    Renderer::destroy();
    GUI::destroy();
}
