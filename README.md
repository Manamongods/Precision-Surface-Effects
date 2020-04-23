WARNING: It is in beta

Wiki is somewhat outdated

https://www.youtube.com/watch?v=95HQabdCR-4

# Precision Surface Effects

Allows different sounds/particles for footsteps and collisions depending on Terrain splats or MeshRenderer materials.

Should easierly work much better for stylized art styles, where the size of a fragment particle doesn't change how it should look. But I did include a simple example shader that scales the detail and offset uv by particle size.

Uses: https://github.com/cfoulston/Unity-Reorderable-List for some reorderability

It's possible that you'll need to import this into an empty project if you want to try the Example Scene, as the free assets I've taken from MicroSplat and Unity's Standard Assets might conflict if you already have them in your project.

This is designed for smooth transitions and sort of automatic (albeit imperfect) support for any situation, without using cutoff points, without thresholds. It's not physically accurate, or very performant (e.g. it uses multiple audio sources for crossfading blending), but it is quite easily made extensible for a large variety of situations if you put in some initial effort. The settings probably seem esoteric, especially for the CollisionEffects component, but the example scene has been set up properly (semi properly; its priority is showcasing features, not best practices) and you can probably learn using it, but otherwise I have filled the Wiki with what I could. Just tell me if there is an area I haven't thought of that is missing support.

More information can be found here: https://forum.unity.com/threads/free-precision-surface-effects-collision-footsteps-sounds-particles-beta.869461/

## Limitations

- Material names need to be proper (only include the relevant keyword, not another SurfaceType's keyword. Otherwise it might find the wrong SurfaceType)
- Performance isn't amazing, so it shouldn't be used too frequently on mobile. Be careful even on PC (especially if you have a lot of CollisionEffects's)

## Follow me on Twitter

https://twitter.com/PrecisionCats

## Attribution

Attribution would be really appreciated, but it's not necessary.
