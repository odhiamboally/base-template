using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Dashboard;

public record AgingBucket(
    int Total,
    double AvgDays,
    int Over14Days,
    int Days7To14,
    int Days3To6,
    int Under3Days);
