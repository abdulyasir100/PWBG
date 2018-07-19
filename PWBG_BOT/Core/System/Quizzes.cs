﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PWBG_BOT.Core.System
{
    public static class Quizzes
    {
        private static List<Quiz> quizzes;

        private static string accountsFile = "Resources/accounts.json";

        static Quizzes()
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

        public static Quiz GetQuiz(SocketUser user)
        {
            return GetOrCreateAccount(user.Id);
        }

        public static void SaveAccount()
        {
            DataStorage.SaveUserAccounts(accounts, accountsFile);
        }

        public static Quiz GetOrCreateAccount(ulong id)
        {
            var result = from a in accounts
                         where a.ID == id
                         select a;
            var account = result.FirstOrDefault();
            if (account == null) account = CreateUserAccount(id);
            return account;
        }

        private static Quiz CreateUserAccount(ulong id)
        {
            var newAccount = new UserAccount()
            {
                ID = id,
                Points = 10,
                XP = 0
            };
            accounts.Add(newAccount);
            SaveAccount();
            return newAccount;
        }
    }
}