using Cysharp.Text;

namespace FellOnline.Shared
{
	public class FBuffAttribute
	{
		public int value;
		public FCharacterAttributeTemplate template;

		public FBuffAttribute(int value, FCharacterAttributeTemplate template)
		{
			this.value = value;
			this.template = template;
		}

		public string Tooltip()
		{
			using (var sb = ZString.CreateStringBuilder())
			{
				sb.Append("<color=#a66ef5>");
				sb.Append(template.Name);
				if (template != null)
				{
					sb.Append(template.name);
					sb.Append(": ");
					sb.Append(value);
				}
				sb.Append("</color>");
				return sb.ToString();
			}
		}
	}
}