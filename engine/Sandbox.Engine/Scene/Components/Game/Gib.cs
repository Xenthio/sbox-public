namespace Sandbox;

/// <summary>
/// A gib is a prop that is treated slightly different. It will fade out after a certain amount of time.
/// </summary>
[Expose]
[Title( "Gib" )]
[Category( "Game" )]
[Icon( "broken_image" )]
public class Gib : Prop
{
	public float FadeTime { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( FadeTime > 0 && !Scene.IsEditor )
		{
			_ = RunGib();
		}
	}

	async Task RunGib()
	{
		await Task.DelaySeconds( FadeTime + Random.Shared.Float( 0, 2.0f ) );

		if ( !IsValid )
			return;

		var modelComponent = Components.Get<ModelRenderer>();
		if ( modelComponent is not null )
		{
			for ( float f = modelComponent.Tint.a; f > 0.0f; f -= Time.Delta )
			{
				modelComponent.Tint = modelComponent.Tint.WithAlpha( f );
				await Task.Frame();
			}
		}

		GameObject.Destroy();
	}

	/// <summary>
	/// Find the gib in <paramref name="gibs"/> whose collider is closest to <paramref name="worldPoint"/>.
	/// Returns null if nothing is within <paramref name="maxDistance"/>. Useful for re-attaching
	/// things that were on the original prop (decals, impact effects, ropes, particles) to the
	/// gib that took that piece of the body.
	/// </summary>
	public static Gib FindAtPoint( IEnumerable<Gib> gibs, Vector3 worldPoint, float maxDistance = 4f )
	{
		if ( gibs is null ) return null;

		Gib best = null;
		var bestDistSqr = maxDistance * maxDistance;

		foreach ( var gib in gibs )
		{
			if ( !gib.IsValid() ) continue;

			var closest = worldPoint;
			var found = false;

			foreach ( var collider in gib.GameObject.GetComponents<Collider>() )
			{
				if ( !collider.IsValid() ) continue;
				var p = collider.FindClosestPoint( worldPoint );
				var d = (p - worldPoint).LengthSquared;
				if ( !found || d < (closest - worldPoint).LengthSquared )
				{
					closest = p;
					found = true;
				}
			}

			if ( !found ) continue;

			var distSqr = (closest - worldPoint).LengthSquared;
			if ( distSqr < bestDistSqr )
			{
				bestDistSqr = distSqr;
				best = gib;
			}
		}

		return best;
	}
}
