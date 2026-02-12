#ifndef PLOTTER_APPLICATION_H
#define PLOTTER_APPLICATION_H

#include "sfw.h"
#include "plotter_scene.h"

class PlotterApplication : public Application {
    SFW_OBJECT(PlotterApplication, Application);

public:
    PlotterApplication();
    ~PlotterApplication();
};

#endif // PLOTTER_APPLICATION_H
