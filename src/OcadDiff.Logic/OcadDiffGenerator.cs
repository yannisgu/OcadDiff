using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;
using OcadParser;
using OcadParser.Models;
using OcadParser.Models.CourseSetting;

namespace OcadDiff.Logic
{
    public class OcadDiffGenerator
    {
        public string SourceFile { get; private set; }
        public string TargetFile { get; private set; }

        public OcadDiffGenerator(string sourceFile, string targetFile)
        {
            SourceFile = sourceFile;
            TargetFile = targetFile;
        }

        public OcadDiff GetDiff()
        {
            var source = new OcadFileReader(SourceFile).ReadProject();
            var target = new OcadFileReader(TargetFile).ReadProject();
            var diff = new OcadDiff() {Source = source, Target = target};
            CompareObjects(source, target, diff);
            return diff;
        }

        private void CompareObjects(OcadBaseProject source, OcadBaseProject target, OcadDiff diff)
        {
            // Copy object lists
            var srcObjects = new List<OcadFileOcadObject>(source.Objects);
            var tarObjects = new List<OcadFileOcadObject>(target.Objects);
            foreach (var obj in srcObjects)
            {
                var foundObject = tarObjects.FirstOrDefault(_ => _.Equals(obj));
                if (foundObject == null)
                {
                    diff.DeletedObjects.Add(obj);
                }
                else
                {
                    tarObjects.Remove(foundObject);
                }
            }

            diff.AddedObjects.AddRange(tarObjects);
        }
        

        /*public OcadDiff GetDiff()
        {
            var diff = new OcadDiff();
            var compareLogic = new CompareLogic();
            compareLogic.Config.MaxDifferences = Int32.MaxValue;
            compareLogic.Config.IgnoreCollectionOrder = true;
            compareLogic.Config.CollectionMatchingSpec[typeof(CourseSettingObject)] = new [] { "Code" };
            diff.Report = compareLogic.Compare(GetProject(SourceFile), GetProject(TargetFile));
            return diff;
        }*/

        private OcadCourseSettingProject GetProject(string sourceFile)
        {
            OcadCourseSettingProject project = new OcadCourseSettingProject();
            using (var stream = new FileStream(sourceFile, FileMode.Open))
            {
                var reader = new OcadStreamReader(stream);
                var parser = new BinaryParser<OcadFile>();

                var file = parser.Read(reader);
                project.Load(file);
            }
            return project;
        }
    }
}
