#include "sfw.h"
#include "plotter_application.h"

int main() {
    Application *app = memnew(PlotterApplication);

    app->start_main_loop();

    memdelete(app);

    return 0;
}
