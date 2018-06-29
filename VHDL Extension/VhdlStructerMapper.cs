using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.NRefactory.TypeSystem;
using Microsoft.VisualStudio.Text;
using VHDL_Extension.Errors;
using VHDL_Extension.Types;

namespace VHDL_Extension
{
    static class VhdlStructerMapper
    {
        private enum MapperState
        {
            SearchStartEntity,
            SearchStartPort,
            SearchPortSignal,
            SearchEndPort,
            SearchEndEntity,
            SearchArchitecture,
            SearchArchitectureSignal,
            SearchProcess
        }

        public static List<VhdlError> VhdlErrors = new List<VhdlError>();

        private const string EnitityText = "entity";
        private const string PortString = "port";
        private const string ArchitectureString = "architecture";

        private static MapperState State { get; set; } = MapperState.SearchStartEntity;

        public static Entity VhdlEntity { get; set; } = new Entity();

        public static void MapVhdl(ITextSnapshotLine line)
        {
            if (line.LineNumber == 0)
            {
                //Reset the state
                State = MapperState.SearchStartEntity;
                VhdlErrors.Clear();
            }

            var text = line.GetText();
            switch (State)
            {
                case MapperState.SearchStartEntity:
                    //Look for the entity beginnin
                    if (text.ToLower().StartsWith(EnitityText))
                    {
                        VhdlEntity.Name = text.Substring(EnitityText.Length + 1).Split(' ')[0];
                        VhdlEntity.StartLine = line.LineNumber;
                        State = MapperState.SearchStartPort;
                        //Here we can also detect stuff like, is there an IS after the entity name and such
                    }
                    break;

                case MapperState.SearchStartPort:
                    if (text.TrimStart().ToLower().StartsWith(PortString))
                    {
                        //Port found
                        VhdlEntity.Port.Signals.Clear();
                        State = MapperState.SearchPortSignal;
                    }
                    break;

                case MapperState.SearchPortSignal:
                    //First get the type and direction
                    var typeAndDirection = text.Split(':')[1].Split(' ');

                    var direction = PortSignal.GetDirection(typeAndDirection[1].ToLower());
                    var type = string.Join(" ", typeAndDirection, 2, typeAndDirection.Length - 2).Trim(';');

                    foreach (var signalName in text.Split(':')[0].Split(','))
                    {
                        PortSignal sig = new PortSignal
                        {
                            Name = signalName.Trim(),
                            Direction = direction,
                            Type = type
                        };
                        VhdlEntity.Port.Signals.Add(sig);
                    }

                    if (!text.EndsWith(";"))
                    {
                        //End of port signal, soo
                        State = MapperState.SearchEndPort;
                    }
                    break;

                case MapperState.SearchEndPort:
                    //Look for end of port, for error checking purposes
                    if (text.Trim() == ");") //End of port should look like this
                    {
                        State = MapperState.SearchEndEntity;
                    }
                    else
                    {
                        //Error occured, or it is on the line down. We'll check if this line contains the end of the entity, so than we now there is an error
                        if (text.ToUpper().Trim().StartsWith("END"))
                        {
                            //ERROR Occured TODO send error message and such
                            if (VhdlErrors.All(e => e.ErrorType != VhdlErrorType.PortEnd))
                            {
                                VhdlErrors.Add(new VhdlError(new SnapshotSpan(line.Start, line.End), VhdlErrorType.PortEnd));
                            }
                        }
                    }
                    break;

                case MapperState.SearchEndEntity:
                    //Search for end of entity, for error checking purposes
                    if (text.StartsWith("END"))
                    {
                        //Found the end of the entity.
                        var name = text.Split(' ');
                        if (name.Length > 1 || VhdlEntity.Name == name[1].Trim(';'))
                        {
                            //Found the end of the entity
                            VhdlEntity.EndLine = line.LineNumber;
                            State = MapperState.SearchArchitecture;
                        }
                    } //TODO add error checking for entity end
                    break;

                case MapperState.SearchArchitecture:
                    //Search for architecture
                    if (text.ToLower().StartsWith(ArchitectureString))
                    {
                        var textLine = text.Split(' ');
                        //Layout: ARCHITECTURE name OF EntityName IS
                        if (textLine.Length != 4 || textLine[2].ToLower() != "of" || textLine[4].ToLower() != "is")
                        {
                            //TODO show error
                        }
                        else if (textLine[3] == VhdlEntity.Name)
                        {
                            VhdlEntity.Architecture.Name = textLine[1];
                        }

                        VhdlEntity.Architecture.Signals.Clear();
                        State = MapperState.SearchArchitectureSignal;
                    }
                    break;

                case MapperState.SearchArchitectureSignal:
                    if (text.TrimStart().ToLower().StartsWith("signal"))
                    {
                        type = text.Split(':')[1].TrimEnd(';');

                        var names = string.Join(",", text.Split(':')[0].Trim().Split(' '), 1, text.Split(':')[0].Trim().Split(' ').Length - 1);

                        foreach (var signalName in names.Split(','))
                        {
                            ArchitectureSignal sig = new ArchitectureSignal
                            {
                                Name = signalName.Trim(),
                                Type = type
                            };
                            VhdlEntity.Architecture.Signals.Add(sig);
                        }

                        if (!text.EndsWith(";"))
                        {
                            //TODO show error
                            var s = new SnapshotSpan(line.Start, line.End);
                            if (VhdlErrors.All(e => e.Span != s))
                            {
                                VhdlErrors.Add(new VhdlError(s, VhdlErrorType.ClosingSemiColon));
                            }
                        }
                    }

                    break;
            }
        }
    }
}
