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

    static String GenerateTestFile (int num_groups, String file_name) {
        String expected_out = "";
        if (!File.Exists(file_name)) {
            StreamWriter writetext = new StreamWriter(file_name);
            writetext.WriteLine("; Assembly listing for method Test Metod");
            writetext.WriteLine("\n");

            for (int i = 0; i < num_groups; i++) {
                String num = GetStringFromNum(i + 1);
                writetext.WriteLine("G_M_IG"+num+":");
                expected_out = expected_out + " " + "IG"+num;
            }

            writetext.Close();
        } else {
            for (int i = 0; i < num_groups; i++) {
                expected_out = expected_out + " " + "IG"+ GetStringFromNum(i + 1);
            }
        }

        return expected_out;
    }

    static void update_counts (bool condition, String test_name, String file_name, ref int passed, ref int failed, ref List<String> failed_test) {
        if (condition) {passed += 1;}
        else {failed += 1; failed_test.Add(test_name + ": " + file_name);}
    }

    static void StressTestAllVerticesDetected(int num_groups, ref int passed, ref int failed, ref List<String> failed_test) {
        String file_name = "test_file/test_stress_"+num_groups+".txt";
        String expected_out = GenerateTestFile(num_groups,file_name);
        String vertexCount = Program.TestAllVerticesDetected(file_name);

        bool condition = vertexCount.Trim() == expected_out.Trim();
        update_counts(condition, "StressTestAllVerticesDetected", file_name, ref passed, ref failed, ref failed_test);
    }

    static void TestSVGHTML(String file, String[] sol_file, ref int passed, ref int failed, ref List<String> failed_test, Boolean html) {
        String [] svg_out = Program.TestSVGorHTMLCreated(file, html);
        for (int i=0; i < sol_file.Length; i++) {
            String svg_sol = File.ReadAllText(Path.GetFullPath(sol_file[i]));

            bool condition = svg_sol.Trim() == svg_out[i].Trim();
            update_counts(condition, "TestSVGorHTMLCreated", file, ref passed, ref failed, ref failed_test);
        }
    }
    
    public static void Main(String[] args) {
        int passed = 0; int failed = 0;
        List<String> failed_test = new List<String>();

        // Test Graph has all Vertices
        StressTestAllVerticesDetected(10, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(30, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(80, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(500, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(1000, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(2000, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(3000, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(4000, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(5000, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(10000, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(20000, ref passed, ref failed, ref failed_test);
        StressTestAllVerticesDetected(9000, ref passed, ref failed, ref failed_test);

        // Cycle tests
        String [] cycle1 = Program.TestCycleDetection("test_file/test1.txt");
        update_counts(Program.TestCycleDetection("test_file/test1.txt").Length == 0, "TestCycleDetection", "test_file/test1.txt", ref passed, ref failed, ref failed_test);
        update_counts(Program.TestCycleDetection("test_file/test_stress_1000.txt").Length == 0, "TestCycleDetection", "test_file/test_stress_1000.txt", ref passed, ref failed, ref failed_test);
        update_counts(Program.TestCycleDetection("test_file/test_stress_2000.txt").Length == 0, "TestCycleDetection", "test_file/test_stress_2000.txt", ref passed, ref failed, ref failed_test);
        update_counts(Program.TestCycleDetection("test_file/test_stress_4000.txt").Length == 0, "TestCycleDetection", "test_file/test_stress_4000.txt", ref passed, ref failed, ref failed_test);
        update_counts(Program.TestCycleDetection("test_file/test_stress_10000.txt").Length == 0, "TestCycleDetection", "test_file/test_stress_10000.txt", ref passed, ref failed, ref failed_test);

        String [] cycle2 = Program.TestCycleDetection("test_file/test2.txt");
        String [] cycle2_sol = new String [] {"IG01 IG03 IG02 IG01","IG02 IG04 IG03 IG02", "IG01 IG05 IG03 IG02 IG01", "IG06 IG06"};
        update_counts(Enumerable.SequenceEqual(cycle2, cycle2_sol), "TestCycleDetection", "Test_file/test2.txt", ref passed, ref failed, ref failed_test);

        // Test SVG File 
        TestSVGHTML("test_file/test1.txt", new String [] {"test_solution/test1.svg"}, ref passed, ref failed, ref failed_test, false);
        TestSVGHTML("test_file/test2.txt", new String [] {"test_solution/test2.svg"}, ref passed, ref failed, ref failed_test, false);
        TestSVGHTML("test_file/test_stress_500.txt", new String [] {"test_solution/test_stress_500.svg"}, ref passed, ref failed, ref failed_test, false);
        TestSVGHTML("test_file/test_stress_1000.txt", new String [] {"test_solution/test_stress_1000.svg"}, ref passed, ref failed, ref failed_test, false);
        TestSVGHTML("test_file/test_stress_2000.txt", new String [] {"test_solution/test_stress_2000.svg"}, ref passed, ref failed, ref failed_test, false);
        TestSVGHTML("test_file/test_colors.txt", new String [] {"test_solution/test_colors.svg"}, ref passed, ref failed, ref failed_test, false);

        String [] multiple_sol_files = new String [] {"test_solution/7fd93a6f.svg", "test_solution/c648fd53.svg", "test_solution/1b5e9e9c.svg", "test_solution/d1234a96.svg"};
        TestSVGHTML("test_file/test_multiple_x64.txt", multiple_sol_files, ref passed, ref failed, ref failed_test, false);

        // Test GTML File 
        TestSVGHTML("test_file/test_stress_2000.txt", new String [] {"test_solution/test_stress_2000.html"}, ref passed, ref failed, ref failed_test, true);
        TestSVGHTML("test_file/test_colors.txt", new String [] {"test_solution/test_colors.html"}, ref passed, ref failed, ref failed_test, true);

        String [] multiple_sol_files_html = new String [] {"test_solution/7fd93a6f.html", "test_solution/c648fd53.html", "test_solution/1b5e9e9c.html", "test_solution/d1234a96.html"};
        TestSVGHTML("test_file/test_multiple_x64.txt", multiple_sol_files_html, ref passed, ref failed, ref failed_test, true);

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