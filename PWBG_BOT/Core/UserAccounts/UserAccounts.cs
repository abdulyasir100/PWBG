﻿using System;
using System.Collections.Generic;
using Discord.WebSocket;
using System.Linq;
using PWBG_BOT.Core.PlayerInventory;
using PWBG_BOT.Core.BuffAndDebuff;
using PWBG_BOT.Core.Items;

namespace PWBG_BOT.Core.UserAccounts
{
    public static class UserAccounts
    {
        //DONT FORGET TO SAVE FILE BY USING UserAccounts.SaveAccount(); AFTER ADDING EXP OR SOMETHING OTHERWISE ITS NOT GONNA BE CHANGED

        private static List<UserAccount> accounts;

        private static string accountsFile = "Resources/accounts.json";

        static UserAccounts()
        {
            if (DataStorage.SaveExist(accountsFile))
            {
                accounts = DataStorage.LoadUserAccounts(accountsFile).ToList();
            }
            else
            {
                accounts = new List<UserAccount>();
                SaveAccount();
            }
        }

        public static void AddingPoints(UserAccount user, int point)
        {
            user.Points += point;
            SaveAccount();
        }

        public static void AddAllPoints(UserAccount user)
        {
            user.Points += user.TempPoint;
            SaveAccount();
        }

        public static void ResetTempPoint(UserAccount user)
        {
            user.TempPoint = 0;
            SaveAccount();
        }

        public static void TempPoints(UserAccount user, int point)
        {
            if (point <= user.TempPoint) return;
            user.TempPoint = point;
            SaveAccount();
        }

        public static void DecreasingPoints(UserAccount user, int point)
        {
            user.Points -= point;
            SaveAccount();
        }

        public static void AddingKills(UserAccount user, uint kill)
        {
            user.Kills += kill;
            SaveAccount();
        }

        public static void IncreasingHealth(UserAccount user, int ammount)
        {
            user.HP += ammount;
            SaveAccount();
        }

        public static async void DecreasingHealth(UserAccount user, int ammount)
        {
            if (Inventories.CheckHaveThisItem(user, "Chainmail") && user.HP - ammount <= 0)
            {
                if (CheckHaveThisBuff(user, "Reversality"))
                {
                    IncreasingHealth(user, ammount);
                    user.Buffs.Remove(Buffs.GetSpecificBuff("Reversality"));
                    await GlobalVar.ChannelSelect.SendMessageAsync("YOUR REVERSALITY BUFF HAS BEEN REMOVED");
                    return;
                }
                user.HP = 1;
                Item getto = Drops.GetSpecificItem("Chainmail");
                SocketUser realuser = GlobalVar.GuildSelect.GetUser(user.ID);
                Inventories.DropAnyItem(realuser, getto);
            }
            else if (Inventories.CheckHaveThisItem(user, "Bulletproof Vest"))
            {
                ammount = (ammount / 2) + 1;
                bool temp = false;
                if (CheckHaveThisBuff(user, "Reversality"))
                {
                    IncreasingHealth(user, ammount);
                    user.Buffs.Remove(Buffs.GetSpecificBuff("Reversality"));
                    await GlobalVar.ChannelSelect.SendMessageAsync("YOUR REVERSALITY BUFF HAS BEEN REMOVED");
                    temp = !temp;
                }
                Item getto = Drops.GetSpecificItem("Bulletproof Vest");
                SocketUser realuser = GlobalVar.GuildSelect.GetUser(user.ID);
                Inventories.DropAnyItem(realuser, getto);
                if (temp) return;
                user.HP -= ammount;
                if (user.HP < 0) user.HP = 0;
            }
            else if (user.HP - ammount < 0)
            {
                if (CheckHaveThisBuff(user, "Reversality"))
                {
                    IncreasingHealth(user, ammount);
                    user.Buffs.Remove(Buffs.GetSpecificBuff("Reversality"));
                    await GlobalVar.ChannelSelect.SendMessageAsync("YOUR REVERSALITY BUFF HAS BEEN REMOVED");
                    return;
                }
                user.HP = 0;
            }
            else
            {
                if (CheckHaveThisBuff(user, "Reversality"))
                {
                    IncreasingHealth(user, ammount);
                    user.Buffs.Remove(Buffs.GetSpecificBuff("Reversality"));
                    await GlobalVar.ChannelSelect.SendMessageAsync("YOUR REVERSALITY BUFF HAS BEEN REMOVED");
                    return;
                }
                user.HP -= ammount;
            }
            SaveAccount();
        }

        public static bool CheckHaveThisBuff(UserAccount user, string name)
        {
            if (user.Buffs.Count <= 0) return false;
            foreach (var b in user.Buffs)
            {
                if (b.Name.Equals(name)) return true;
            }
            return false;
        }

        public static UserAccount GetUserAccount(SocketUser user)
        {
            return GetOrCreateAccount(user.Id);
        }
        
        public static UserAccount GetUserAccountByID(ulong id)
        {
            var result = from a in accounts
                         where a.ID == id
                         select a;
            var account = result.FirstOrDefault();
            if (account == null) return null;
            return account;
        }
        
        public static List<UserAccount> GetAllUsers()
        {
            return accounts;
        }

        public static List<UserAccount> GetAllAliveUsers()
        {
            List<UserAccount> alive = new List<UserAccount>();
            foreach (var a in accounts)
            {
                if (a.HP <= 0) continue;
                alive.Add(a);
            }
            return alive;
        }

        public static Inventory GetInventory(SocketUser user)
        {
            return Inventories.GetOrCreateInventory(user.Id);
        }

        public static void SaveAccount()
        {
            DataStorage.SaveUserAccounts(accounts, accountsFile);
        }

        private static UserAccount GetOrCreateAccount(ulong id)
        {
            var result = from a in accounts
                         where a.ID == id
                         select a;
            var account = result.FirstOrDefault();
            if (account == null) account = CreateUserAccount(id);
            return account;
        }

        private static UserAccount CreateUserAccount(ulong id)
        {
            var newAccount = new UserAccount()
            {
                ID = id,    
                Points = 0,
                Buffs = new List<Buff>(),
                Debuffs = new List<Debuff>(),
                HP = 15,
                Inventory = Inventories.GetOrCreateInventory(id),
                Kills = 0
            };
            accounts.Add(newAccount);
            SaveAccount();
            return newAccount;
        }
        
        public static UserAccount GetRandomPlayer(SocketGuild guild)
        {
            var users = guild.Users;
            List<UserAccount> randomPlayers = new List<UserAccount>();
            var role = from r in guild.Roles
                       where r.Name.Equals("Player")
                       select r;
            var des = role.FirstOrDefault();
            foreach (var u in users)
            {
                if (u.Roles.Contains(des))
                {
                    var user = UserAccounts.GetUserAccount((SocketUser)u);
                    if (user.HP <= 0) continue;
                    randomPlayers.Add(user);
                }
            }
            Random gacha = new Random();
            if (randomPlayers.Count == 0) return null;
            int luckyIndex = (int)gacha.Next(0,randomPlayers.Count);
            Console.WriteLine(luckyIndex);
            return randomPlayers[luckyIndex];
        }

        public static Buff GetRandomBuff(UserAccount target)
        {
            if (target.Buffs.Count == 1) return target.Buffs[0];
            Random rand = new Random();
            int luckyBuff = rand.Next(0,target.Buffs.Count);
            return target.Buffs[luckyBuff];
        }

        public static async void GiveBuff(UserAccount user, Item item, SocketTextChannel channel)
        {
            foreach (var b in item.Buffs)
            {
                if (user.Buffs.Count >= 3) return;
                user.Buffs.Add(b);
                await channel.SendMessageAsync($"YOU GOT {b.Name} BUFF");
                SaveAccount();
            }
        }

        public static UserAccount GetRandomBesideMe(UserAccount me)
        {
            UserAccount target;
            if (accounts.Count<=0)
            {
                return null;
            }
            do
            { target = GetRandomPlayer(GlobalVar.GuildSelect); }
            while (me == target);
            return target;
        }

        public static bool IsDead(SocketUser user)
        {
            UserAccount account = GetUserAccount(user);
            if (account.HP <= 0) return true;
            return false;
        }

        public static void StatusAilment(UserAccount user)
        {
            if (user.Debuffs.Count<=0) return;
            foreach (var d in user.Debuffs)
            {
                switch (d.Name)
                {
                    case "Burn":
                        StatusAilments.Burn(user, d);
                        break;
                    default:
                        StatusAilments.DecreaseDebuffCountDown(user,d);
                        break;
                    //more status ailment later
                }
            }
        }

    }
}
