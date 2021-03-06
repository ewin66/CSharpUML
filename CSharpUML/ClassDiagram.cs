using System;
using System.Collections.Generic;
using System.Linq;

using NDesk.Options;

namespace CSharpUML
{
	public class ClassDiagram
	{
		private IUmlObject[] objects;

		public ClassDiagram (IEnumerable<IUmlObject> objects)
		{
			this.objects = objects.ToArray ();
		}

		private string EscapeLines (string line, int lineLength)
		{
			string[] parts = line.Trim ().Split (" ");
			string escaped = "";
			bool first = true;
			int length = 0;
			foreach (string part in parts) {
				if (!first) {
					escaped += " ";
					length += 1;
				}
				escaped += Escape (part);
				length += part.Length;
				first = false;

				if (length > lineLength) {
					escaped += "\\l";
					length = 0;
					first = true;
				}
			}
			escaped += "\\l";
			return escaped;
		}

		public IEnumerable<string> DotCode (string param = "", string backgroundColor = "", int lineLength = 80)
		{
			yield return "digraph \"MenuItem\"";
			yield return "{";
			yield return "  edge [fontname=\"Arial\",fontsize=\"8\",labelfontname=\"Lucida\",labelfontsize=\"8\"];";
			yield return "  node [fontname=\"Arial\",fontsize=\"8\",shape=record];";

			foreach (IUmlObject obj in objects) {
				string[] umlcode = obj.ToUmlCode ().Split ("\n").Where (
					(l) => !l.Contains ("//") && !l.Contains ("Attributes") && !l.Contains ("Methods")
				).ToArray ();
				string code = "Box_" + obj.Name.Clean () + " [label=\"{" + Escape (obj.Name) + "\\n|";
				for (int i = 1; i < umlcode.Length; ++i) {
					if (UmlAttribute.Matches (new UmlBlock (name: umlcode [i], comments: new string[]{}))) {
						code += EscapeLines (umlcode [i], lineLength);
					}
				}
				//if (i > 1 && i + 1 < umlcode.Length) {
				code += "|";
				//}
				for (int i = 1; i < umlcode.Length; ++i) {
					if (!UmlAttribute.Matches (new UmlBlock (name: umlcode [i], comments: new string[]{}))) {
						code += EscapeLines (umlcode [i], lineLength);
					}
				}
				string color = backgroundColor != "" ? backgroundColor
					: obj is UmlClass && (obj as UmlClass).type == ClassType.Interface ? "dafcda"
					: "fcfcda";
				code += "}\",height=0.2,width=0.4,color=\"black\", fillcolor=\"#" + color + "\"," + //
					"style=\"filled\", fontcolor=\"black\"];\n";
				yield return code;
			}

			List<Inheritance> inheritances;
			List<string> unknownObjects;
			Inheritance.From (objects, out inheritances, out unknownObjects);

			foreach (string name in unknownObjects) {
				string code = "Box_" + name.Clean () + " [label=\"{" + Escape (name) + "\\n}\"";
				code += ",height=0.2,width=0.4,color=\"black\", fillcolor=\"#ffffff\"," +
					"style=\"filled\" fontcolor=\"black\"];\n";
				yield return code;
			}

			foreach (Inheritance inh in inheritances) {
				yield return "Box_" + inh.Base.Name.Clean () + " -> Box_" + inh.Derived.Name.Clean () +
					" [dir=\"back\",color=\"midnightblue\",fontsize=\"8\",style=\"solid\"," +
					"arrowtail=\"onormal\",fontname=\"Helvetica\"];";
			}
			yield return param;
			yield return "}";
		}

		private static string Escape (string str)
		{
			return str.TrimAll ().Replace ("\n", "\\l")
				.Replace ("{", "\\{").Replace ("}", "\\}")
				.Replace ("[", "\\[").Replace ("]", "\\]")
				.Replace ("<", "\\<").Replace (">", "\\>");
		}
	}
}