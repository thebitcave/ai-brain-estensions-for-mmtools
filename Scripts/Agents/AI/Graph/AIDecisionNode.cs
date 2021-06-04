﻿using MoreMountains.Tools;
using UnityEngine;

namespace TheBitCave.MMToolsExtensions.AI.Graph
{
	/// <summary>
	/// A node representing a single Corgi <see cref="MoreMountains.Tools.AIDecision"/>.
	/// </summary>
	[NodeWidth(300)]
	[NodeTint("#447dC3")]
	[CreateNodeMenu("")]
	public class AIDecisionNode : AINode
	{
		/// <summary>
		/// The Corgi Decision label.
		/// </summary>
		public string label;

		[Output(connectionType = ConnectionType.Multiple)] public DecisionConnection output;
		
		
		public virtual AIDecision AddDecisionComponent(GameObject go)
		{
			throw new System.NotImplementedException();
		}
	}
}