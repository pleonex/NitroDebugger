// (c) Copyright, Real-Time Innovations, 2018.
// All rights reserved.
//
// No duplications, whole or partial, manual or electronic, may be made
// without express written permission.  Any such copies, or
// revisions thereof, must display this notice unaltered.
// This code contains trade secrets of Real-Time Innovations, Inc.
using System;
using Xwt;
using NitroDebugger.RSP;
using Gee.External.Capstone;
using Gee.External.Capstone.Arm;
using System.Text;
using Xwt.Drawing;

namespace NitroDebugger.UI
{
    public class MainWindow : Window
    {
        Button playBtn;
        Button pauseBtn;
        Button stepInBtn;
        Button stepOverBtn;
        Button connectBtn;
        Button stopBtn;

        MarkdownView assemblyView;
        MarkdownView registerView;
        MarkdownView hexView1;
        MarkdownView hexView2;

        readonly GdbClient gdb;
        CapstoneDisassembler<
            ArmInstruction,
            ArmRegister,
            ArmInstructionGroup,
            ArmInstructionDetail> disassembler;

        public MainWindow()
        {
            gdb = new GdbClient();
            gdb.Execution.BreakExecution += (sender, signal) => Update();
            CreateComponents();
        }

        void CreateComponents()
        {
            Table table = new Table();

            // Add the buttons
            HBox topbar = new HBox();
            playBtn = new Button("Play");
            playBtn.Clicked += (sender, e) => gdb.Execution.Continue();
            topbar.PackStart(playBtn);

            pauseBtn = new Button("Pause");
            pauseBtn.Clicked += delegate {
                gdb.Execution.Stop();
                Update(); 
            };
            topbar.PackStart(pauseBtn);

            stepInBtn = new Button("StepIn");
            topbar.PackStart(stepInBtn);

            stepOverBtn = new Button("StepOver");
            topbar.PackStart(stepOverBtn);

            connectBtn = new Button("Connect");
            connectBtn.Clicked += Connect;
            topbar.PackEnd(connectBtn);

            stopBtn = new Button("Stop");
            topbar.PackEnd(stopBtn);

            table.Add(topbar, 0, 0, 1, 2, true);

            // Add the assembly view
            assemblyView = new MarkdownView();
            assemblyView.Font = Font.SystemMonospaceFont;
            table.Add(assemblyView, 0, 1, 1, 1, true, true);

            registerView = new MarkdownView();
            registerView.Font = Font.SystemMonospaceFont;
            table.Add(registerView, 1, 1, 1, 1, true, true);

            // Add the two hexadecimal displays
            hexView1 = new MarkdownView();
            hexView1.Font = Font.SystemMonospaceFont;
            hexView1.ReadOnly = true;
            table.Add(hexView1, 0, 2, 1, 1, true, true);

            hexView2 = new MarkdownView();
            hexView2.Font = Font.SystemMonospaceFont.WithSize(8);
            hexView2.ReadOnly = true;
            table.Add(hexView2, 1, 2, 1, 1, true, true);

            Content = table;
            Width = 1200;
            Height = 600;
            Title = "NitroDebugger alpha ~~ by pleonex ~~";

            CloseRequested += (sender, args) => Application.Exit();
        }

        void Connect(object sender, EventArgs e)
        {
            Console.WriteLine(Environment.CurrentDirectory);
            gdb.Connection.Connect("localhost", 23947);
            disassembler = CapstoneDisassembler.CreateArmDisassembler(
                DisassembleMode.Arm32);
        }

        void Update()
        {
            uint pc = gdb.Registers.GetRegister(RegisterType.PC).Value;
            byte[] assemblyBytes = gdb.Stream.Read(pc - 0x20, 0x40);

            StringBuilder assemblyBuilder = new StringBuilder();
            foreach (var inst in disassembler.DisassembleAll(assemblyBytes, pc)) {
                assemblyBuilder.Append($"*{inst.Address:X8}h*    ");
                assemblyBuilder.Append($"**{inst.Mnemonic}**".PadRight(16));
                assemblyBuilder.AppendLine($"{inst.Operand}  ");
            }
            assemblyView.Markdown = assemblyBuilder.ToString();

            hexView1.Markdown = ShowHex(assemblyBytes, pc);

            byte[] hexData2 = gdb.Stream.Read(0x02E00800, 0x100);
            hexView2.Markdown = ShowHex(hexData2, 0x02E00800);

            StringBuilder registerBuilder = new StringBuilder();
            foreach (Register reg in gdb.Registers.GetRegisters()) {
                registerBuilder.Append($"**{reg.Type}:**".PadRight(8));
                registerBuilder.AppendLine($"{reg.Value:X8}  ");
            }
            registerView.Markdown = registerBuilder.ToString();
        }

        string ShowHex(byte[] data, uint pos)
        {
            const int BytesPerRow = 0x10;

            StringBuilder hexBuilder = new StringBuilder();
            hexBuilder.Append("Offset".PadRight(13));
            for (int i = 0; i < BytesPerRow; i++)
                hexBuilder.AppendFormat(" {0:X2}", i);
            hexBuilder.AppendLine("  ");
            hexBuilder.AppendLine("  ");

            int numRows = 0x10;
            bool eof = false;
            int relativePos = 0;
            for (int r = 0; r < numRows && !eof; r++) {
                hexBuilder.AppendFormat("0x{0:X8}   ", (pos + r) * BytesPerRow);

                var asciiBuilder = new StringBuilder("   ");
                for (int c = 0; c < BytesPerRow && !eof; c++) {
                    if (relativePos >= data.Length) {
                        eof = true;
                        break;
                    }

                    byte value = data[relativePos++];
                    hexBuilder.AppendFormat(" {0:X2}", value);
                    if (value > 0x1F && value < 0x7F)
                        asciiBuilder.Append(" " + (char)value);
                    else
                        asciiBuilder.Append(" .");
                }

                hexBuilder.Append(asciiBuilder.ToString());
                if (r != numRows - 1)
                    hexBuilder.AppendLine("  ");
            }

            return hexBuilder.ToString();
        }
    }
}
