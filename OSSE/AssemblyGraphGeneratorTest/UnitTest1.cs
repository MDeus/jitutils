
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssemblyGraph;
using AssemblyGraphGenerator;
// Nuget MSTest.TestFramework;

namespace AssemblyGraphGeneratorTest;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestVertex()
    {
        String[] lines = File.ReadAllLines("./test_file1"); 
        List<Graph> graphs = AssemblyGraphGenerator.Program.createGraphs(lines, true);
        Assert.AreEqual(1, graphs.Count);
        Assert.AreEqual(6, graphs[0].getVertices().Count);

    }
}