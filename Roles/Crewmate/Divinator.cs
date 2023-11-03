using Hazel;
using System.Collections.Generic;
using System.Linq;
using static TOHE.Options;
using static TOHE.Translator;

namespace TOHE.Roles.Crewmate;

public static class Divinator
{
    private static readonly int Id = 6700;
    private static List<byte> playerIdList = new();

    public static OptionItem CheckLimitOpt;
    public static OptionItem AccurateCheckMode;
    public static OptionItem HideVote;
    public static OptionItem ShowSpecificRole;
    public static OptionItem AbilityUseGainWithEachTaskCompleted;

    public static List<byte> didVote = new();
    public static Dictionary<byte, float> CheckLimit = new();

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Divinator);
        CheckLimitOpt = IntegerOptionItem.Create(Id + 10, "DivinatorSkillLimit", new(0, 20, 1), 1, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Divinator])
            .SetValueFormat(OptionFormat.Times);
        AccurateCheckMode = BooleanOptionItem.Create(Id + 12, "AccurateCheckMode", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Divinator]);
        ShowSpecificRole = BooleanOptionItem.Create(Id + 13, "ShowSpecificRole", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Divinator]);
        HideVote = BooleanOptionItem.Create(Id + 14, "DivinatorHideVote", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Divinator]);
        AbilityUseGainWithEachTaskCompleted = FloatOptionItem.Create(Id + 15, "AbilityUseGainWithEachTaskCompleted", new(0f, 5f, 0.1f), 1f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Divinator])
            .SetValueFormat(OptionFormat.Times);
        OverrideTasksData.Create(Id + 21, TabGroup.CrewmateRoles, CustomRoles.Divinator);
    }
    public static void Init()
    {
        playerIdList = new();
        CheckLimit = new();
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        CheckLimit.TryAdd(playerId, CheckLimitOpt.GetInt());
    }
    public static bool IsEnable => playerIdList.Any();
    public static void SendRPC(byte playerId, bool isMinus)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetDivinatorLimit, SendOption.Reliable, -1);
        writer.Write(playerId);
        writer.Write(isMinus);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC(MessageReader reader)
    {
        if (AmongUsClient.Instance.AmHost) return;

        byte playerId = reader.ReadByte();
        bool isMinus = reader.ReadBoolean();

        if (isMinus) CheckLimit[playerId]--;
        else CheckLimit[playerId]++;
    }
    public static void OnVote(PlayerControl player, PlayerControl target)
    {
        if (player == null || target == null) return;
        if (didVote.Contains(player.PlayerId)) return;
        didVote.Add(player.PlayerId);

        if (CheckLimit[player.PlayerId] < 1)
        {
            Utils.SendMessage(GetString("DivinatorCheckReachLimit"), player.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Divinator), GetString("DivinatorCheckMsgTitle")));
            return;
        }

        CheckLimit[player.PlayerId] -= 1;
        SendRPC(player.PlayerId, true);

        if (player.PlayerId == target.PlayerId)
        {
            Utils.SendMessage(GetString("DivinatorCheckSelfMsg") + "\n\n" + string.Format(GetString("DivinatorCheckLimit"), CheckLimit[player.PlayerId]), player.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Divinator), GetString("DivinatorCheckMsgTitle")));
            return;
        }

        string msg;

        if ((player.AllTasksCompleted() || AccurateCheckMode.GetBool()) && ShowSpecificRole.GetBool())
        {
            msg = string.Format(GetString("DivinatorCheck.TaskDone"), target.GetRealName(), GetString(target.GetCustomRole().ToString()));
        }
        else
        // Investigator
        {
            string text = target.GetCustomRole() switch
            {

                CustomRoles.CrewmateTOHE or
                CustomRoles.EngineerTOHE or
                CustomRoles.ScientistTOHE or
                CustomRoles.ImpostorTOHE or
                CustomRoles.ShapeshifterTOHE
                => "Result0",

                CustomRoles.Amnesiac or
                CustomRoles.CopyCat or
                CustomRoles.Eraser or
                CustomRoles.AntiAdminer or
                CustomRoles.Monitor or
                CustomRoles.Dazzler or
                CustomRoles.Grenadier or
                CustomRoles.Lighter
                => "Result1",

                CustomRoles.Crusader or
                CustomRoles.Farseer or
                CustomRoles.Arsonist or
                CustomRoles.Assassin or
                CustomRoles.Undertaker or
                CustomRoles.BallLightning or
                CustomRoles.Collector
                => "Result2",

                CustomRoles.Capitalism or
                //CustomRoles.Counterfeiter or
                CustomRoles.Witness or
                CustomRoles.Greedier or
                CustomRoles.Ventguard or
                CustomRoles.Merchant or
                CustomRoles.Trickster
                => "Result3",

                CustomRoles.Pestilence or
                CustomRoles.PlagueBearer or
                CustomRoles.Observer or
                CustomRoles.BloodKnight or
                CustomRoles.Guardian or
                CustomRoles.Wildling
                => "Result4",

                CustomRoles.Bard or
                CustomRoles.Juggernaut or
                //CustomRoles.Reverie or
                CustomRoles.SecurityGuard or
                CustomRoles.Sans or
                CustomRoles.Ricochet or
                CustomRoles.Minimalism or
                CustomRoles.OverKiller
                => "Result5",

                CustomRoles.Bloodhound or
                CustomRoles.EvilTracker or
                CustomRoles.Mortician or
                CustomRoles.Stealth or
                CustomRoles.Tracefinder or
                CustomRoles.Romantic or
                CustomRoles.Tracker
                => "Result6",


                CustomRoles.Bodyguard or
                CustomRoles.Bomber or
                CustomRoles.FireWorks or
                CustomRoles.Lookout or
                CustomRoles.RuthlessRomantic or
                CustomRoles.VengefulRomantic or
                CustomRoles.Nuker
                => "Result7",

                CustomRoles.BountyHunter or
                CustomRoles.Detective or
                CustomRoles.FFF or
                CustomRoles.Cleaner or
                CustomRoles.Medusa or
                CustomRoles.Psychic
                => "Result8",

                CustomRoles.Convict or
                CustomRoles.Executioner or
                CustomRoles.Lawyer or
                CustomRoles.Snitch or
                CustomRoles.Penguin or
                CustomRoles.Disperser or
                CustomRoles.Doctor
                => "Result9",

                CustomRoles.Councillor or
                CustomRoles.Dictator or
                CustomRoles.Judge or
                CustomRoles.CursedSoul or
                CustomRoles.Cleanser or
                CustomRoles.CursedWolf
                => "Result10",

                CustomRoles.Addict or
                CustomRoles.Alchemist or
                CustomRoles.Escapee or
                CustomRoles.Miner or
                CustomRoles.Demolitionist or
                CustomRoles.Ventguard or
                CustomRoles.Morphling
                => "Result11",

                CustomRoles.Gamer or
                CustomRoles.Zombie or
                CustomRoles.CyberStar or
                CustomRoles.SuperStar or
                CustomRoles.Doormaster or
                CustomRoles.Deathpact or
                CustomRoles.Devourer
                => "Result12",

                CustomRoles.God or
                CustomRoles.Oracle or
                CustomRoles.NiceSwapper or
                CustomRoles.Visionary or
                CustomRoles.Gambler or
                CustomRoles.NiceEraser or
                CustomRoles.ParityCop
                => "Result13",

                CustomRoles.Hacker or
                CustomRoles.Mayor or
                CustomRoles.Paranoia or
                CustomRoles.Pickpocket or
                CustomRoles.Mastermind or
                CustomRoles.Spy or
                CustomRoles.Vindicator
                => "Result14",

                CustomRoles.Infectious or
                CustomRoles.Virus or
                CustomRoles.Monarch or
                CustomRoles.Revolutionist or
                CustomRoles.Agitater or
                CustomRoles.Express or
                CustomRoles.Succubus
                => "Result15",

                CustomRoles.Innocent or
                CustomRoles.Refugee or
                CustomRoles.Inhibitor or
                CustomRoles.Sapper or
                CustomRoles.SabotageMaster or
                CustomRoles.Bandit or
                CustomRoles.Saboteur
                => "Result16",

                CustomRoles.Medic or
                CustomRoles.Mario or
                CustomRoles.Jester or
                CustomRoles.RiftMaker or
                CustomRoles.Lurker or
                CustomRoles.Glitch or
                CustomRoles.Sunnyboy
                => "Result17",

                CustomRoles.Mafia or
                CustomRoles.Retributionist or
                CustomRoles.Gangster or
                CustomRoles.Glitch or
                CustomRoles.Hitman or
                CustomRoles.Luckey or
                CustomRoles.Underdog
                => "Result18",

                CustomRoles.EvilGuesser or
                CustomRoles.NiceGuesser or
                CustomRoles.DarkHide or
                CustomRoles.Reckless or
                CustomRoles.Camouflager or
                CustomRoles.Eclipse or
                CustomRoles.Chameleon
                => "Result19",

                CustomRoles.Jackal or
                CustomRoles.Sidekick or
                CustomRoles.Maverick or
                CustomRoles.WeaponMaster or
                CustomRoles.Opportunist or
                CustomRoles.Pursuer or
                CustomRoles.Provocateur
                => "Result20",

                CustomRoles.Poisoner or
                CustomRoles.Vampire or
                CustomRoles.DovesOfNeace or
                CustomRoles.ImperiusCurse or
                CustomRoles.Magician or
                CustomRoles.HeadHunter or
                CustomRoles.Traitor
                => "Result21",

                CustomRoles.BoobyTrap or
                CustomRoles.QuickShooter or
                CustomRoles.NSerialKiller or
                CustomRoles.Werewolf or
                CustomRoles.Sheriff or
                CustomRoles.Admirer or
                CustomRoles.Warlock
                => "Result22",

                CustomRoles.Divinator or
                CustomRoles.EvilDiviner or
                CustomRoles.Ritualist or
                CustomRoles.Postman or
                CustomRoles.Imitator or
                CustomRoles.HexMaster or
                CustomRoles.Witch
                => "Result23",

                CustomRoles.Needy or
                CustomRoles.Totocalcio or
                CustomRoles.Pelican or
                CustomRoles.Scavenger or
                CustomRoles.NiceHacker or
                CustomRoles.Vengeance or
                CustomRoles.Vulture
                => "Result24",

                CustomRoles.Jinx or
                CustomRoles.SwordsMan or
                CustomRoles.Veteran or
                CustomRoles.Enigma or
                CustomRoles.TaskManager or
                CustomRoles.Pyromaniac or
                CustomRoles.Hangman
                => "Result25",

                CustomRoles.Mediumshiper or
                CustomRoles.Spiritcaller or
                CustomRoles.Spiritualist or
                CustomRoles.Parasite or
                CustomRoles.CameraMan or
                CustomRoles.Swooper or
                CustomRoles.Wraith
                => "Result26",

                CustomRoles.TimeManager or
                CustomRoles.TimeMaster or
                CustomRoles.TimeThief or
                CustomRoles.PlagueDoctor or
                CustomRoles.ShapeMaster or
                CustomRoles.Tether or
                CustomRoles.Sniper
                => "Result27",

                CustomRoles.Puppeteer or
                //CustomRoles.NWitch or
                CustomRoles.Deputy or
                CustomRoles.Transporter or
                CustomRoles.Twister or
                CustomRoles.Aid or
                CustomRoles.SerialKiller
                => "Result28",

                CustomRoles.Crewpostor or
                CustomRoles.Marshall or
                CustomRoles.Workaholic or
                CustomRoles.Phantom or
                CustomRoles.Speedrunner or
                CustomRoles.Terrorist
                => "Result29",



                _ => "Null",
            };
            msg = string.Format(GetString("DivinatorCheck." + text), target.GetRealName());
        }
        // Fortune Teller
        /*   {
               string text = target.GetCustomRole() switch
               {
                   CustomRoles.TimeThief or
                   CustomRoles.AntiAdminer or
                   CustomRoles.SuperStar or
                   CustomRoles.Mayor or
                   CustomRoles.Vindicator or
                   CustomRoles.Snitch or
                   CustomRoles.Marshall or
                   CustomRoles.Counterfeiter or
                   CustomRoles.God or
                   CustomRoles.Judge or
                   CustomRoles.Observer or
                   CustomRoles.DovesOfNeace or
                   CustomRoles.Virus
                   => "HideMsg",

                   CustomRoles.Miner or
                   CustomRoles.Scavenger or
                   CustomRoles.Luckey or
                   CustomRoles.Trickster or
                   CustomRoles.Needy or
                   CustomRoles.SabotageMaster or
                   CustomRoles.EngineerTOHE or
                   CustomRoles.Jackal or
                   CustomRoles.Parasite or
                   CustomRoles.Impostor or
               //    CustomRoles.Sidekick or
                   CustomRoles.Mario or
                   CustomRoles.Cleaner or
                   CustomRoles.Crewpostor or
                   CustomRoles.Disperser
                   => "Honest",

                   CustomRoles.SerialKiller or
                   CustomRoles.BountyHunter or
                   CustomRoles.Minimalism or
                   CustomRoles.Sans or
                   CustomRoles.Juggernaut or
                   CustomRoles.SpeedBooster or
                   CustomRoles.Sheriff or
                   CustomRoles.Arsonist or
                   CustomRoles.Innocent or
                   CustomRoles.FFF or
                   CustomRoles.Greedier or
                   CustomRoles.Tracker
                   => "Impulse",

                   CustomRoles.Vampire or
                   CustomRoles.Poisoner or
                   CustomRoles.Assassin or
                   CustomRoles.Escapee or
                   CustomRoles.Sniper or
                   CustomRoles.NSerialKiller or
                   CustomRoles.SwordsMan or
                   CustomRoles.Bodyguard or
                   CustomRoles.Opportunist or
                   CustomRoles.Pelican or
                   CustomRoles.ImperiusCurse
                   => "Weirdo",

                   CustomRoles.EvilGuesser or
                   CustomRoles.Bomber or
                   CustomRoles.Capitalism or
                   CustomRoles.NiceGuesser or
                   CustomRoles.Grenadier or
                   CustomRoles.Lighter or
                   CustomRoles.Terrorist or
                   CustomRoles.Revolutionist or
                   CustomRoles.Gamer or
                   CustomRoles.Eraser or
                   CustomRoles.Farseer
                   => "Blockbuster",

                   CustomRoles.Warlock or
                   CustomRoles.Hacker or
                   CustomRoles.Mafia or
                   CustomRoles.Retributionist or
                   CustomRoles.Doctor or
                   CustomRoles.ScientistTOHE or
                   CustomRoles.Transporter or
                   CustomRoles.Veteran or
                   CustomRoles.Divinator or
                   CustomRoles.QuickShooter or
                   CustomRoles.Mediumshiper or
                   CustomRoles.Judge or
                   CustomRoles.Wildling or
                   CustomRoles.BloodKnight
                   => "Strong",

                   CustomRoles.Witch or
                   CustomRoles.HexMaster or
                   CustomRoles.Puppeteer or
                   CustomRoles.NWitch or
                   CustomRoles.ShapeMaster or
                   CustomRoles.ShapeshifterTOHE or
                   CustomRoles.Paranoia or
                   CustomRoles.Psychic or
                   CustomRoles.Executioner or
                   CustomRoles.Lawyer or
                   CustomRoles.BallLightning or
                   CustomRoles.Workaholic or
                   CustomRoles.Provocateur
                   => "Incomprehensible",

                   CustomRoles.FireWorks or
                   CustomRoles.EvilTracker or
                   CustomRoles.Gangster or
                   CustomRoles.Dictator or
                   CustomRoles.CyberStar or
                   CustomRoles.Demolitionist or
                   CustomRoles.NiceEraser or
                   CustomRoles.TaskManager or
                   CustomRoles.Collector or
                   CustomRoles.Sunnyboy or
                   CustomRoles.Bard or
                   CustomRoles.Totocalcio or
                   CustomRoles.Bloodhound
                   => "Enthusiasm",

                   CustomRoles.BoobyTrap or
                   CustomRoles.Zombie or
                   CustomRoles.Mare or
                   CustomRoles.Detective or
                   CustomRoles.TimeManager or
                   CustomRoles.Jester or
                   CustomRoles.Medicaler or
                   CustomRoles.GuardianAngelTOHE or
                   CustomRoles.DarkHide or
                   CustomRoles.CursedWolf or
                   CustomRoles.OverKiller or
                   CustomRoles.Hangman or
                   CustomRoles.Mortician or
                   CustomRoles.Spiritcaller
                   => "Disturbed",

                   CustomRoles.Glitch or
                   CustomRoles.Camouflager or
                   CustomRoles.Wraith or
                   CustomRoles.Swooper
                   => "Glitch",

                   CustomRoles.Succubus
                   => "Love",

                   _ => "None",
               };
               msg = string.Format(GetString("DivinatorCheck." + text), target.GetRealName());
           }*/

        Utils.SendMessage(GetString("DivinatorCheck") + "\n" + msg + "\n\n" + string.Format(GetString("DivinatorCheckLimit"), CheckLimit[player.PlayerId]), player.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Divinator), GetString("DivinatorCheckMsgTitle")));
    }
}