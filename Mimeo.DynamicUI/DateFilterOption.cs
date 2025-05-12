using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mimeo.DynamicUI
{
    public enum DateFilterOption
    {
        /// <summary>
        /// Refers to a <see cref="DateTime"/> representation of today, relative to <see cref="DateDisplayMode"/>.
        /// </summary>
        Today,

        /// <summary>
        /// Refers to a <see cref="DateTime"/> representation of yesterday, relative to <see cref="DateDisplayMode"/>.
        /// </summary>
        Yesterday,

        /// <summary>
        /// Refers to a <see cref="DateTime"/> representation of tomorrow, relative to <see cref="DateDisplayMode"/>.
        /// </summary>
        Tomorrow,

        /// <summary>
        /// Refers to a <see cref="DateTime"/> representation of one week or seven days ago, relative to <see cref="DateDisplayMode"/>.
        /// </summary>
        SevenDaysAgo,

        /// <summary>
        /// Refers to a <see cref="DateTime"/> representation of one week or seven days from now, relative to <see cref="DateDisplayMode"/>.
        /// </summary>
        SevenDaysFromNow,

        /// <summary>
        /// Refers to a <see cref="DateTime"/> representation of approximately one month or thirty days ago, relative to <see cref="DateDisplayMode"/>.
        /// </summary>
        ThirtyDaysAgo,

        /// <summary>
        /// Refers to a <see cref="DateTime"/> representation of approximately one month or thirty days from now, relative to <see cref="DateDisplayMode"/>.
        /// </summary>
        ThirtyDaysFromNow,

        /// <summary>
        /// Refers to a <see cref="DateTime"/> representation of a variable amount of days from today's current date, relative to <see cref="DateDisplayMode"/>.
        /// </summary>
        XDaysFromNow,

        /// <summary>
        /// Refers to an exact <see cref="DateTime"/> instance, representing a specific day.
        /// </summary>
        Exact,
    }
}
