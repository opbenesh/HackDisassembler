using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HackDisassembler
{
    class Program
    {
        static void Main(string[] args)
        {
			if(args.Length != 1)
			{
				Console.WriteLine("Usage: HackDisassembler [file]");
				return;
			}
            var path = args[0];
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (line.Length == 0) continue;
                Console.WriteLine(Instruction.Dissasemble(Convert.ToUInt16(line,2)));
            }
            Console.ReadKey();
        }
    }
    abstract class Instruction
    {
        public static Instruction Dissasemble(int instruction)
        {
            if(instruction>>15==0)
                return new AInstruction(instruction);
            return new CInstruction(instruction);
        }
    }
    class AInstruction : Instruction
    {
        public int Value { get; private set; }
        public AInstruction(int instruction)
        {
            this.Value = instruction;
        }
        public override string ToString()
        {
            return string.Format("@{0}", Value);
        }
    }
    [Flags]
    enum Jump
    {
        Jlt = 0x4, Jeq = 0x2, Jgt = 0x1
    }
    [Flags]
    enum Destination
    {
        A=0x4, D=0x2, M=0x1
    }
    enum AluIn1
    {
        A=0, M=1
    }
    class CInstruction : Instruction
    {
        public Jump Jump { get; private set; }
        public Destination Destination { get; private set; }
        public int AluOperation { get; private set; }
        public AluIn1 AluIn1 { get; private set; }
        public CInstruction(int instruction)
        {
            Jump= (Jump)(instruction & 7);
            instruction >>= 3;
            Destination = (Destination)(instruction & 7);
            instruction >>= 3;
            AluOperation = instruction & 63;
            instruction >>= 6;
            AluIn1=(AluIn1)(instruction & 1);
        }
        private static Dictionary<int, string> AluOpToFormat = new Dictionary<int, string>()
        {
            {42,"0"},
            {63,"1"},
            {58,"-1"},
            {12,"D"},
            {48,"{0}"},
            {13,"!D"},
            {49,"!{0}"},
            {15,"-D"},
            {51,"-{0}"},
            {31,"D+1"},
            {55,"{0}+1"},
            {14,"D-1"},
            {50,"{0}-1"},
            {2,"D+{0}"},
            {19,"D-{0}"},
            {7,"{0}-D"},
            {0,"D&{0}"},
            {21,"D|{0}"}
        };
        private static Dictionary<Jump, string> JumpToString = new Dictionary<Jump, string>()
        {
            {0,null},
            {Jump.Jlt | Jump.Jeq | Jump.Jgt,"JMP"},
            {Jump.Jlt | Jump.Jeq,"JLE"},
            {Jump.Jlt, "JLT"},
            {Jump.Jeq | Jump.Jgt,"JGE"},
            {Jump.Jeq, "JEQ"},
            {Jump.Jgt, "JGT"},
            {Jump.Jlt | Jump.Jgt,"JNE"}
        };
        public override string ToString()
        {
            var sb = new StringBuilder();
            var dest = DestinationToString();
            sb.Append(dest);
            if (dest.Length > 0)
                sb.Append("=");
            sb.AppendFormat(AluOpToFormat[AluOperation], AluIn1.ToString());
            if (Jump > 0)
                sb.AppendFormat(";{0}",JumpToString[Jump]);
            return sb.ToString();
        }

        private string DestinationToString()
        {
            var sb = new StringBuilder();
            foreach (var flag in new[] { Destination.A, Destination.D, Destination.M })
                if (Destination.HasFlag(flag)) sb.Append(flag.ToString());
            return sb.ToString();
        }
    }
}
