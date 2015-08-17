using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;
using OcadParser;
using OcadParser.Models;

namespace OcadDiff.Logic
{
    public class OcadDiff
    {
        public ComparisonResult Report { get; set; }

        public OcadBaseProject Source { get; set; }
        public OcadBaseProject Target { get; set; }

        public List<OcadFileOcadObject> DeletedObjects { get; } = new List<OcadFileOcadObject>();
        public List<OcadFileOcadObject> AddedObjects { get;  } = new List<OcadFileOcadObject>();
        public List<OcadFileOcadObject> ModifiedObjects { get;  } = new List<OcadFileOcadObject>();
    }
}
