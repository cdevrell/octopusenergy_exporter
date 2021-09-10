using System;
using System.Collections.Generic;

namespace OctopusEnergyExporter
{

    public class Root
    {
        public int count { get; set; }
        public string next { get; set; }
        public object previous { get; set; }
        public List<Result> results { get; set; }
    }
    
    public class Result
    {
        public double consumption { get; set; }
        public DateTimeOffset interval_start { get; set; }
        public DateTimeOffset interval_end { get; set; }
    }
}