﻿using System.Collections.Generic;
using Plato.Internal.Badges.Abstractions;
using Plato.Internal.Models.Badges;

namespace Plato.Discuss.Tags.Badges
{
    public class TagBadges : IBadgesProvider<Badge>
    {

        public static readonly Badge FirstTag =
            new Badge("First Tag", "Added a topic tag", "fal fa-tag", BadgeLevel.Bronze, 1);
        
        public static readonly Badge BronzeTag =
            new Badge("Tagger", "Added {threshold} topic tags", "fal fa-tag", BadgeLevel.Bronze, 10, 2);

        public static readonly Badge SilverTag =
            new Badge("Tag Artist", "Added {threshold} topic tags", "fal fa-tag", BadgeLevel.Silver, 25, 10);

        public static readonly Badge GoldTAg =
            new Badge("Taxonomist", "Added {threshold} topic tags", "fal fa-tag", BadgeLevel.Gold, 50, 25);
        
        public IEnumerable<Badge> GetBadges()
        {
            return new[]
            {
                FirstTag,
                BronzeTag,
                SilverTag,
                GoldTAg
            };

        }
        
    }

}