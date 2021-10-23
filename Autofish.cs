using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Autofish
{
    public class Autofish : Mod
    {
        public static ModHotKey AutocastKeybind;
        public static ModHotKey LockcastDirectionKeybind;

        public override void Load()
        {
            IL.Terraria.Projectile.FishingCheck += Projectile_FishingCheck;
            AutocastKeybind = RegisterHotKey("Toggle Auto Cast Fishing Pole", "Y");
            LockcastDirectionKeybind = RegisterHotKey("Lock Casting Target To Cursor", "L");
        }

        public override void Unload()
        {
            AutocastKeybind = null;
            LockcastDirectionKeybind = null;
            IL.Terraria.Projectile.FishingCheck -= Projectile_FishingCheck;
        }

        private void Projectile_FishingCheck(ILContext il)
        {
            /* ����1.3��1.4��IL���ǲ�һ����
             * IL_0FEE: ldloc.s V_7
             * IL_0FF0: ldloc.s V_14
             * IL_0FF2: ldloc.s V_26
             * IL_0FF4: ldloca.s V_13
             * IL_0FF6: ldloca.s V_27
             * IL_0FF8: call      void Terraria.ModLoader.PlayerHooks::CatchFish(class Terraria.Player, class Terraria.Item, int32, int32, int32, int32, int32, int32&, bool&)
             * IL_0FFD: ldloc.s V_13
             * ����Ƿ����0��Ȼ���PullTimer
             */

            ILCursor iLCursor = new ILCursor(il);
            iLCursor.GotoNext(MoveType.After, (Instruction i) => ILPatternMatchingExt.MatchLdloc(i, 7));
            iLCursor.GotoNext(MoveType.After, (Instruction i) => ILPatternMatchingExt.MatchLdloc(i, 14));
            iLCursor.GotoNext(MoveType.After, (Instruction i) => ILPatternMatchingExt.MatchLdloc(i, 26));
            iLCursor.GotoNext(MoveType.After, (Instruction i) => ILPatternMatchingExt.MatchLdloca(i, 13));
            iLCursor.GotoNext(MoveType.After, (Instruction i) => ILPatternMatchingExt.MatchLdloca(i, 27));
            iLCursor.GotoNext(MoveType.After, (Instruction i) => ILPatternMatchingExt.MatchCall(i, typeof(PlayerHooks), "CatchFish"));
            iLCursor.GotoNext(MoveType.After, (Instruction i) => ILPatternMatchingExt.MatchLdloc(i, 13));
            iLCursor.Emit(OpCodes.Ldarg_0); // ���뵱ǰProjectileʵ��
            iLCursor.EmitDelegate<Func<int, Projectile, int>>((returnValue, projectile) => {
                if (returnValue > 0 && ModContent.GetInstance<Configuration>().AutoCatchToggle) {
                    Main.player[projectile.owner].GetModPlayer<AutofishPlayer>().PullTimer = (int)(ModContent.GetInstance<Configuration>().PullingDelay * 60 + 1);
                }
                return returnValue; // ��ô������ô��
            });
        }
    }

    public class Configuration : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [Header("Autofish Configuration")]

        [Label("Auto Catch Toggle")]
        [Tooltip("Enables or disables pulling bobbers automatically.")]
        public bool AutoCatchToggle;

        [Label("Auto Catch Pulling Delay (Seconds)")]
        [Tooltip("Set the delay of pulling bobber up after detecting available fish.")]
        [Range(0f, 1.5f)]
        [Increment(.1f)]
        [DefaultValue(.1f)]
        public float PullingDelay;
    }
}