using System.Collections.Generic;
using Cysharp.Text;

namespace FellOnline.Shared
{
	public class FBuff
	{
		private float tickTime;
		private List<FBuffAttribute> attributeBonuses = new List<FBuffAttribute>();
		private List<FBuff> stacks = new List<FBuff>();

		public float RemainingTime;
		public FBuffTemplate Template { get; private set; }
		public List<FBuffAttribute> AttributeBonuses { get { return attributeBonuses; } }
		public List<FBuff> Stacks { get { return stacks; } }

		public FBuff(int templateID)
		{
			Template = FBuffTemplate.Get<FBuffTemplate>(templateID);
			tickTime = Template.TickRate;
			RemainingTime = Template.Duration;
		}

		public FBuff(int templateID, float remainingTime)
		{
			Template = FBuffTemplate.Get<FBuffTemplate>(templateID);
			tickTime = Template.TickRate;
			RemainingTime = remainingTime;
		}

		public FBuff(int templateID, float remainingTime, List<FBuff> stacks)
		{
			Template = FBuffTemplate.Get<FBuffTemplate>(templateID);
			tickTime = Template.TickRate;
			RemainingTime = remainingTime;
			this.stacks = stacks;
		}

		public void SubtractTime(float time)
		{
			RemainingTime -= time;
		}

		public void AddTime(float time)
		{
			RemainingTime += time;
		}

		public void SubtractTickTime(float time)
		{
			tickTime -= time;
		}

		public void AddTickTime(float time)
		{
			tickTime += time;
		}

		public void TryTick(Character target)
		{
			if (tickTime <= 0.0f)
			{
				Template.OnTick(this, target);
				ResetTickTime();
			}
		}

		public void ResetDuration()
		{
			RemainingTime = Template.Duration;
		}

		public void ResetTickTime()
		{
			tickTime = Template.TickRate;
		}

		private void Reset()
		{
			attributeBonuses.Clear();
		}

		public void AddAttributeBonus(FBuffAttribute buffAttributeInstance)
		{
			attributeBonuses.Add(buffAttributeInstance);
		}

		public void Apply(Character target)
		{
			Template.OnApply(this, target);
		}

		public void Remove(Character target)
		{
			Template.OnRemove(this, target);
			Reset();
		}

		public void AddStack(FBuff stack, Character target)
		{
			Template.OnApplyStack(stack, target);
			stacks.Add(stack);
		}

		public void RemoveStack(Character target)
		{
			Template.OnRemoveStack(this, target);
		}

		public string Tooltip()
		{
			using (var sb = ZString.CreateStringBuilder())
			{
				sb.Append("<size=120%><color=#f5ad6e>");
				sb.Append(Template.Name);
				sb.Append("</color></size>");
				sb.AppendLine();
				sb.Append("<color=#a66ef5>Remaining Time: ");
				sb.Append(RemainingTime);
				sb.Append("</color>");
				if (attributeBonuses != null)
				{
					foreach (FBuffAttribute attribute in attributeBonuses)
					{
						sb.AppendLine();
						sb.Append(attribute.Tooltip());
					}
				}
				return sb.ToString();
			}
		}
	}
}