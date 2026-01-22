// Assets/TheWorkingCat/Runtime/Simulation/Agents/CharacterBehavior.cs
namespace TheWorkingCat.Simulation.Agents
{
	public abstract class CharacterBehavior
	{
		protected readonly GridCharacterController controller;

		protected CharacterBehavior(GridCharacterController controller)
		{
			this.controller = controller;
		}

		public virtual void OnEnter() { }
		public virtual void OnExit() { }

		/// <summary>
		/// Decision-making step (runs every frame).
		/// Do not move transforms here. Queue intents on the controller.
		/// </summary>
		public abstract void TickBehavior(float dt);
	}
}
