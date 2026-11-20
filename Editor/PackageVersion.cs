using System;
using System.Text.RegularExpressions;

namespace Framework.Editor
{
    public readonly struct PackageVersion : IComparable<PackageVersion>
    {
        private static readonly Regex Pattern = new Regex(@"^\s*v?(\d+)\.(\d+)\.(\d+)(-[0-9A-Za-z.\-]+)?(\+[0-9A-Za-z.\-]+)?\s*$");

        private readonly string _preRelease;

        private PackageVersion(int major, int minor, int patch, string preRelease)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            _preRelease = preRelease;
        }

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public bool IsPreRelease => !string.IsNullOrEmpty(_preRelease);

        public static bool TryParse(string text, out PackageVersion version)
        {
            version = default;
            var match = text == null ? Match.Empty : Pattern.Match(text);
            if (!match.Success)
            {
                return false;
            }

            version = new PackageVersion(
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value),
                match.Groups[4].Value.TrimStart('-'));
            return true;
        }

        public bool IsAtLeast(PackageVersion required)
        {
            return CompareTo(required) >= 0;
        }

        public int CompareTo(PackageVersion other)
        {
            var byNumbers = Compare(Major, other.Major);
            byNumbers = byNumbers != 0 ? byNumbers : Compare(Minor, other.Minor);
            byNumbers = byNumbers != 0 ? byNumbers : Compare(Patch, other.Patch);
            if (byNumbers != 0)
            {
                return byNumbers;
            }

            if (IsPreRelease == other.IsPreRelease)
            {
                return string.CompareOrdinal(_preRelease, other._preRelease);
            }

            return IsPreRelease ? -1 : 1;
        }

        private static int Compare(int left, int right)
        {
            return left.CompareTo(right);
        }
    }
}
