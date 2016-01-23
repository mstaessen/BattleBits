using System;
using System.Collections.Generic;

namespace BattleBits.Web.Models
{
    /// <summary>
    /// TODO Fake EF Context for testing purposes, make real EF class later
    /// </summary>
    public class CompetitionContext : IDisposable
    {

        public static ISet<Competition> Competitions { get; set; } = new HashSet<Competition> {
            new Competition {
                Name = "Development Game",
                NumberCount = 24
            }
        };

        public void Dispose()
        {

        }
    }
}