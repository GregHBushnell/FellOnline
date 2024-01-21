namespace FellOnline.Shared
{
	public class FItemAttribute
	{
		public FItemAttributeTemplate Template { get; private set; }

		public int value;

		public FItemAttribute(int templateID, int value)
		{
			Template = FItemAttributeTemplate.Get<FItemAttributeTemplate>(templateID);
			this.value = value;
		}
	}
}