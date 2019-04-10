using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Coverlet.Core;

namespace Coverlet.Cmdlet
{
    public class AssemblyCoverageHelper
    {
        public static List<AssemblyData> ConvertCoverageData(CoverageResult result, bool IncludeSummary = true)
        {
            List<AssemblyData> adc = new List<AssemblyData>();
            AssemblyData summary = null;
            if ( IncludeSummary ) {
                summary = new AssemblyData() {
                    Identifier = Guid.Parse(result.Identifier),
                    AssemblyName = "Summary",
                };
            }
            foreach (string key in result.Modules.Keys)
            {
                AssemblyData ad = new AssemblyData() {
                    Identifier = Guid.Parse(result.Identifier),
                    AssemblyName = key,
                };
                ad.AddFileData(result.Modules[key]);
                adc.Add(ad);
                if ( IncludeSummary ) {
                    summary.FileCoverage.AddRange(ad.FileCoverage);
                }
            }
            if ( IncludeSummary ) {
                adc.Add(summary);
            }
            return adc;

        }
    }
    /// <summary>
    /// Describes a .NET assembly available in coverlet coverage
    /// </summary>
    public class AssemblyData
    {
        /// <summary>
        /// The operation identifier
        /// </summary>
        public Guid Identifier { get; set; }

        /// <summary>
        /// the assembly name
        /// <summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// The collection of File Coverage
        /// </summary>
        public List<FileCoverage> FileCoverage { get; set; }

        public AssemblyData()
        {
            FileCoverage = new List<FileCoverage>();
        }

        public void AddFileData(Documents d)
        {
            foreach(string key in d.Keys)
            {
                FileCoverage fc = new FileCoverage() {
                    FileName = key
                };
                fc.GetClasses(d[key]);
                FileCoverage.Add(fc);
            }
        }

        public double Coverage
        {
            get {
            return ((double)HitCount/(double)HitableLines);
            }
        }

        private int _hitableLines = -1;

        public int HitableLines {
            get {
                if ( _hitableLines == -1 ) {
                    _hitableLines = 0;
                    foreach ( FileCoverage fc in FileCoverage ){
                        _hitableLines += fc.HitableLines;
                    }
                }
                return _hitableLines;
            }
        }

        private int _hitCount = -1;
        public int HitCount {
            get {
                if (_hitCount == -1) {
                    _hitCount = 0;
                    foreach ( FileCoverage fc in FileCoverage ){
                        _hitCount += fc.HitCount;
                    }
                }
                return _hitCount;
            }
        }
    }

    public class FileCoverage
    {
        public FileCoverage()
        {
            ClassCoverage = new List<ClassCoverage>();
        }
        public string FileName { get; set; }
        public List<ClassCoverage> ClassCoverage { get; set; }

        public void GetClasses(Classes c)
        {
            foreach(string key in c.Keys)
            {
                ClassCoverage classinfo = new ClassCoverage() {
                    ClassName = key,
                };
                classinfo.GetMethod(c[key]);
                ClassCoverage.Add(classinfo);
            }

        }
        public double GetLineCoverage()
        {
            return ((double)HitCount/(double)HitableLines);
        }

        public double GetBranchCoverage()
        {
            return 1.2;
        }

        public int HitableLines {
            get {
                int i = 0;
                foreach ( ClassCoverage cc in ClassCoverage) {
                    i += cc.HitableLines;
                }
                return i;
            }
        }
        public int HitCount {
            get {
                int i = 0;
                foreach ( ClassCoverage cc in ClassCoverage) {
                    i += cc.HitCount;
                }
                return i;
            }
        }
    }

    public class ClassCoverage
    {
        public string ClassName;
        public List<MethodCoverage> MethodCoverage;

        public ClassCoverage()
        {
            MethodCoverage = new List<MethodCoverage>();
        }

        public void GetMethod(Methods m)
        {
            foreach (string key in m.Keys )
            {
                MethodCoverage mc = new MethodCoverage() {
                    MethodName = key
                };
                mc.GetLinesAndBranches(m[key]);
                MethodCoverage.Add(mc);
            }
        }

        public int HitableLines { 
            get {
                int i = 0;
                foreach ( MethodCoverage mc in MethodCoverage ) {
                    i += mc.HitableLines;
                }
                return i;
            }
        }

        public int HitCount {
            get {
                int i = 0;
                foreach ( MethodCoverage mc in MethodCoverage ) {
                    i += mc.HitCount;
                }
                return i;
            }
        }

        public double GetCoverage()
        {
            return (double)HitCount/(double)HitableLines;
        }
    }

    public class MethodCoverage
    {
        public string MethodName { get; set; }
        public List<LineCoverage> LineCoverage { get; set; }
        public List<BranchCoverage> BranchCoverage {get; set; }

        public MethodCoverage()
        {
            LineCoverage = new List<LineCoverage>();
            BranchCoverage = new List<BranchCoverage>();
        }
        public void GetLinesAndBranches(Method m)
        {
            foreach ( KeyValuePair<int,int> lineData in m.Lines)
            {
                LineCoverage lc = new LineCoverage() {
                    LineNumber = lineData.Key,
                    HitCount = lineData.Value,
                };
                LineCoverage.Add(lc);
            }
            foreach (BranchInfo b in m.Branches )
            {
                BranchCoverage bc = new BranchCoverage() {
                    Line = b.Line,
                    Offset = b.Offset,
                    EndOffset = b.EndOffset,
                    Path = b.Path,
                    Ordinal = b.Ordinal,
                    HitCount = b.Hits,
                };
                BranchCoverage.Add(bc);
            }
        }
        public int HitableLines {
            get {
                return LineCoverage.Count;
            }
        }
        public int HitCount {
            get {
                int hc = 0;
                foreach ( var l in LineCoverage ) {
                    if ( l.HitCount != 0 ) hc++;
                }
                return hc;
            }
        }

        public double Coverage {
            get {
                return ((double)HitCount/(double)HitableLines);
            }
        }
    }

    public class LineCoverage
    {
        public int LineNumber { get; set; }
        public int HitCount { get; set; }
    }

    public class BranchCoverage
    {
            public int Line { get; set; }
            public int Offset { get; set; }
            public int EndOffset { get; set; }
            public int Path {get; set; }
            public uint Ordinal { get; set; }
            public int HitCount { get; set; }

    }
}