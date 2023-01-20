using System;

namespace Server.Utils
{
	public class Template
	{
		private static String _dir = "";
        public static string TemplateDirectory { get => _dir; set => _dir = value; }

		#region Multiton
		private static Dictionary<String, Template> _templates = new Dictionary<String, Template>();

		public static Template Get(String file)
		{
			if (!Template._templates.ContainsKey(file))
			{
				Template._templates.Add(file, new Template(file));
			}

			return Template._templates[file];
		}
		#endregion

		private String _content;

        private Template(String file)
		{
			using (StreamReader f = new StreamReader(Path.Join(Template.TemplateDirectory, file)))
			{
				this._content = f.ReadToEnd();
			}
		}

		public String Render()
			=> this._content;
		public String Render(object? arg0)
			=> String.Format(this._content, arg0);
		public String Render(object? arg0, object? arg1)
			=> String.Format(this._content, arg0, arg1);
		public String Render(object? arg0, object? arg1, object? arg2)
			=> String.Format(this._content, arg0, arg1, arg2);
		public String Render(object? arg0, object? arg1, object? arg2, object? arg3)
			=> String.Format(this._content, arg0, arg1, arg2, arg3);
	}
}
