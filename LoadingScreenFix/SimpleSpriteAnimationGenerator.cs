using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LoadingScreenFix
{
    /// <summary>
    /// Static class used for creating SimpleSpriteAnimations at runtime.
    /// </summary>
    public static class SimpleSpriteAnimationGenerator
    {
        /// <summary>
        /// Creates a <see cref="SimpleSpriteAnimation"/> utilizing a collection of <paramref name="durations"/>, <paramref name="sprites"/>, and a specified <paramref name="framerate"/>
        /// <br>The actual number of frames in the animation is determined by the count of <paramref name="sprites"/>, if <paramref name="durations"/> doesnt match its count, it'll use the last valid count in the collection.</br>
        /// </summary>
        /// <param name="sprites">The sprites to use for the animation</param>
        /// <param name="durations">The duration of each sprite relative to <paramref name="framerate"/></param>
        /// <param name="framerate">The framerate for the animation</param>
        /// <returns>The created sprite animation</returns>
        public static SimpleSpriteAnimation CreateSpriteAnimation(IEnumerable<Sprite> sprites, IEnumerable<int> durations, float framerate)
        {
            var spriteArray = sprites.ToArray();
            var durationsArray = durations.ToArray();

            var spriteCount = spriteArray.Length;
            SimpleSpriteAnimation.Frame[] frames = new SimpleSpriteAnimation.Frame[spriteCount];

            for(int i = 0; i < spriteCount; i++)
            {
                int duration = 1;
                if(HG.ArrayUtils.IsInBounds(durationsArray, i))
                {
                    duration = durationsArray[i];
                }

                frames[i] = new SimpleSpriteAnimation.Frame
                {
                    duration = duration,
                    sprite = spriteArray[i]
                };
            }

            return CreateInternal(new SimpleSpriteAnimationArguments
            {
                framerate = framerate,
                frames = frames
            });
        }

        /// <summary>
        /// Creates a <see cref="SimpleSpriteAnimation"/> utilizing a collection tuple of <see cref="Sprite"/> and <see cref="int"/> within <paramref name="spritesAndDurations"/>, and a specified <paramref name="framerate"/>
        /// </summary>
        /// <param name="spritesAndDurations">The sprites and durations of each sprite</param>
        /// <param name="framerate">The framerate of the animation</param>
        /// <returns>The created sprite animation</returns>
        public static SimpleSpriteAnimation CreateSpriteAnimation(IEnumerable<(Sprite, int)> spritesAndDurations, float framerate)
        {
            var toArray = spritesAndDurations.ToArray();
            var count = toArray.Length;
            SimpleSpriteAnimation.Frame[] frames = new SimpleSpriteAnimation.Frame[count];
            for(int i = 0; i < count; i++)
            {
                frames[i] = new SimpleSpriteAnimation.Frame
                {
                    sprite = toArray[i].Item1,
                    duration = toArray[i].Item2
                };
            }
            return CreateInternal(new SimpleSpriteAnimationArguments
            {
                frames = frames,
                framerate = framerate
            });
        }

        /// <summary>
        /// Creates a <see cref="SimpleSpriteAnimation"/> utilizing an array of <see cref="SimpleSpriteAnimation.Frame"/> within <paramref name="frames"/>, and a specified <paramref name="framerate"/>
        /// </summary>
        /// <param name="frames">The frames for the animation</param>
        /// <param name="framerate">The framerate of the animation</param>
        /// <returns>The created sprite animation</returns>
        public static SimpleSpriteAnimation CreateSpriteAnimation(SimpleSpriteAnimation.Frame[] frames, float framerate)
        {
            return CreateInternal(new SimpleSpriteAnimationArguments
            {
                frames = frames,
                framerate = framerate
            });
        }

        /// <summary>
        /// Creates a <see cref="SimpleSpriteAnimation"/> utilizing <see cref="SimpleSpriteAnimationArguments"/> passed in <paramref name="args"/>
        /// </summary>
        /// <param name="args">The argument to create the animation</param>
        /// <returns>The created sprite animation</returns>
        public static SimpleSpriteAnimation CreateSpriteAnimation(SimpleSpriteAnimationArguments args)
        {
            return CreateInternal(args);
        }

        private static SimpleSpriteAnimation CreateInternal(SimpleSpriteAnimationArguments args)
        {
            var instance = SimpleSpriteAnimation.CreateInstance<SimpleSpriteAnimation>();

            instance.frames = args.frames;
            instance.frameRate = args.framerate;
            return instance;
        }
    }

    /// <summary>
    /// Represents arguments for creating a SimpleSpriteAnimation
    /// </summary>
    public struct SimpleSpriteAnimationArguments
    {
        /// <summary>
        /// The Frames for the sprite animation
        /// </summary>
        public SimpleSpriteAnimation.Frame[] frames;

        /// <summary>
        /// The framerate of the animation
        /// </summary>
        public float framerate;
    }
}