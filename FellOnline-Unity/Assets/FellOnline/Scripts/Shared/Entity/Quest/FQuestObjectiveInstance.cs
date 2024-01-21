namespace FellOnline.Shared
{
	public class FQuestObjectiveInstance
	{
		public FQuestObjective template;

		private long value;

		public bool IsComplete()
		{
			return template != null && this.value >= template.RequiredValue;
		}
	}
}