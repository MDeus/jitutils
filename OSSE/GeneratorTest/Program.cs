// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssemblyGraphGenerator;

public class Test {
    static String GetStringFromNum(int i) {
        String num="";
        if(i < 10) {
            num = "0" + i.ToString();
        } else {
            num = i.ToString();
        }

        return num;
    }

    static void generateTestFile(int num_groups) {

    }

    static void StressTestAllVerticesDetected(int num_groups, ref int passed, ref int failed, ref List<String> failed_test) {
        String file_name = "test_file/test_stress_"+num_groups+".txt";
        String expected_out = "";
        if (!File.Exists(file_name)) {
            Random rnd = new Random();
            StreamWriter writetext = new StreamWriter(file_name);
            writetext.WriteLine("; Assembly listing for method Test Metod");
            writetext.WriteLine("\n");

            for (int i = 0; i < num_groups; i++) {
                String num = GetStringFromNum(i + 1);
                writetext.WriteLine("G_M_IG"+num+":");
                writetext.WriteLine("\tcbz G_M_IG"+GetStringFromNum(rnd.Next(1, num_groups)));

                expected_out = expected_out + " " + "IG"+num;
            }

            writetext.Close();
        } else {
            for (int i = 0; i < num_groups; i++) {
                expected_out = expected_out + " " + "IG"+ GetStringFromNum(i + 1);
            }
        }

        String vertexCount = Program.TestAllVerticesDetected(file_name);
        if (vertexCount.Trim() == expected_out.Trim()) {passed += 1;} else {failed += 1; failed_test.Add("StressTestAllVerticesDetected: " + file_name);}
    }

    static void TestSVG(String file, String[] sol_file, ref int passed, ref int failed, ref List<String> failed_test) {
        String [] svg_out = Program.TestSVGCreated(file);
        for (int i=0; i < sol_file.Length; i++) {
            String svg_sol = File.ReadAllText(Path.GetFullPath(sol_file[i]));
            if (svg_sol.Trim() == svg_out[i].Trim()) {passed += 1;} else {failed += 1; failed_test.Add("TestSVGCreated: " + file);}
        }
    }
    
    public static void Main(String[] args) {
        int passed = 0; int failed = 0;
        List<String> failed_test = new List<String>();

        // Test Graph has all Vertices
        // StressTestAllVerticesDetected(10, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(10, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(30, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(80, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(500, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(1000, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(10000, ref passed, ref failed, ref failed_test);

        // Cycle tests
        String [] cycle1 = Program.TestCycleDetection("test_file/test1.txt");
        if (cycle1.Length == 0) {passed += 1;} else {failed += 1; failed_test.Add("TestCycleDetection: test1.txt");}

        String [] cycle2 = Program.TestCycleDetection("test_file/test2.txt");
        String [] cycle2_out = new String [] {"IG01 IG03 IG02 IG01","IG01 IG05 IG03 IG02 IG01","IG06 IG06", "IG02 IG04 IG03 IG02"};
        if (Enumerable.SequenceEqual(cycle2, cycle2_out)) {passed += 1;} else {failed += 1; failed_test.Add("TestCycleDetection: test2.txt");}

        // Test SVG File 
        TestSVG("test_file/test1.txt", new String [] {"test_solution/test1.svg"}, ref passed, ref failed, ref failed_test);
        TestSVG("test_file/test2.txt", new String [] {"test_solution/test2.svg"}, ref passed, ref failed, ref failed_test);
        TestSVG("test_file/test3.txt", new String [] {"test_solution/test3.svg"}, ref passed, ref failed, ref failed_test);
        TestSVG("test_file/test_stress_30.txt", new String [] {"test_solution/test_stress_30.svg"}, ref passed, ref failed, ref failed_test);

        Console.WriteLine("\n\n\n");
        // Program.TestCycleDetection("test_file/test_stress_30.txt");

        // print Test Results
        Console.Write("\nTESTS RAN: {0}", passed + failed);
        Console.Write(" PASSED: {0}", passed);
        Console.Write(" FAILED: {0}", failed);
        if (failed > 0) {
            Console.WriteLine("\n\nFailed Tests");
            Console.WriteLine("-------------------------------------------------");
            foreach(String f in failed_test) {Console.WriteLine(f);}
        }
        Console.WriteLine();
    }
}