﻿using System;
using Sandbox;

namespace Minigolf
{
	public class FreeCamera : Camera
	{
		Angles LookAngles;
		Vector3 MoveInput;

		Vector3 TargetPos;
		Rotation TargetRot;

		float MoveSpeed;
		float LerpMode = 0;

		public FreeCamera()
		{
			TargetPos = CurrentView.Position;
			TargetRot = CurrentView.Rotation;

			Position = TargetPos;
			Rotation = TargetRot;
			LookAngles = Rotation.Angles();
		}

		public override void Update()
		{
			var player = Local.Client;
			if ( player == null ) return;

			var tr = Trace.Ray( Position, Position + Rotation.Forward * 4096 ).UseHitboxes().Run();

			// DebugOverlay.Box( tr.EndPos, Vector3.One * -1, Vector3.One, Color.Red );

			Viewer = null;
			{
				var lerpTarget = tr.EndPos.Distance( Position );

				DoFPoint = lerpTarget;// DoFPoint.LerpTo( lerpTarget, Time.Delta * 10 );
			}

			FreeMove();
		}

		public override void BuildInput( InputBuilder input )
		{
			MoveInput = input.AnalogMove;

			MoveSpeed = 1;
			if ( input.Down( InputButton.Run ) ) MoveSpeed = 5;
			if ( input.Down( InputButton.Duck ) ) MoveSpeed = 0.2f;

			if ( input.Down( InputButton.Slot1 ) ) LerpMode = 0.0f;
			if ( input.Down( InputButton.Slot2 ) ) LerpMode = 0.5f;
			if ( input.Down( InputButton.Slot3 ) ) LerpMode = 0.9f;

			if ( input.Down( InputButton.Use ) )
				DoFBlurSize = Math.Clamp( DoFBlurSize + (Time.Delta * 3.0f), 0.0f, 100.0f );

			if ( input.Down( InputButton.Menu ) )
				DoFBlurSize = Math.Clamp( DoFBlurSize - (Time.Delta * 3.0f), 0.0f, 100.0f );

			LookAngles += input.AnalogLook;
			LookAngles.roll = 0;

			input.ClearButton( InputButton.Attack1 );

			input.StopProcessing = true;
		}

		void FreeMove()
		{
			var mv = MoveInput.Normal * 300 * RealTime.Delta * Rotation * MoveSpeed;

			TargetRot = Rotation.From( LookAngles );
			TargetPos += mv;

			Position = Vector3.Lerp( Position, TargetPos, 10 * RealTime.Delta * (1 - LerpMode) );
			Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );
		}
	}
}
