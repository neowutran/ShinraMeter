using System;
using System.Collections.Generic;

namespace Tera.DamageMeter
{
    public class DamagePlaceHolders
    {
        public static PlaceHolder FromPlayerInfo(PlayerInfo playerInfo, FormatHelpers formatHelpers)
        {
            var placeHolders = new List<KeyValuePair<string, object>>();
            placeHolders.Add(new KeyValuePair<string, object>("Name", playerInfo.Name));
            placeHolders.Add(new KeyValuePair<string, object>("Class", playerInfo.Class));

            placeHolders.Add(new KeyValuePair<string, object>("Crits", playerInfo.Dealt.Crits));
            placeHolders.Add(new KeyValuePair<string, object>("Hits", playerInfo.Dealt.Hits));

            placeHolders.Add(new KeyValuePair<string, object>("DamagePercent", formatHelpers.FormatPercent(playerInfo.DamageFraction) ?? "-"));
            placeHolders.Add(new KeyValuePair<string, object>("CritPercent", formatHelpers.FormatPercent((double)playerInfo.Dealt.Crits / playerInfo.Dealt.Hits) ?? "-"));

            placeHolders.Add(new KeyValuePair<string, object>("Damage", formatHelpers.FormatValue(playerInfo.Dealt.Damage)));
            placeHolders.Add(new KeyValuePair<string, object>("DamageReceived", formatHelpers.FormatValue(playerInfo.Received.Damage)));
            placeHolders.Add(new KeyValuePair<string, object>("DPS", formatHelpers.FormatValue(playerInfo.Dps)));
            return new PlaceHolder(placeHolders, formatHelpers.CultureInfo);
        }
    }
}
