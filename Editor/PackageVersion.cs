using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Framework.Editor
{
    public readonly struct PackageVersion : IComparable<PackageVersion>
    {
        private static readonly Regex Pattern = new Regex(@"^\s*v?([0-9]{1,9})\.([0-9]{1,9})\.([0-9]{1,9})(-[0-9A-Za-z.\-]+)?(\+[0-9A-Za-z.\-]+)?\s*$");

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

            if (IsPreRelease != other.IsPreRelease)
            {
                return IsPreRelease ? -1 : 1;
            }

            return ComparePreReleases(_preRelease, other._preRelease);
        }

        private static int ComparePreReleases(string left, string right)
        {
            var leftParts = (left ?? string.Empty).Split('.');
            var rightParts = (right ?? string.Empty).Split('.');
            var shared = Math.Min(leftParts.Length, rightParts.Length);
            for (var index = 0; index < shared; index++)
            {
                var byPart = ComparePreReleasePart(leftParts[index], rightParts[index]);
                if (byPart != 0)
                {
                    return byPart;
                }
            }

            return leftParts.Length.CompareTo(rightParts.Length);
        }

        private static int ComparePreReleasePart(string left, string right)
        {
            var leftIsNumber = IsNumber(left);
            var rightIsNumber = IsNumber(right);
            if (leftIsNumber && rightIsNumber)
            {
                var trimmedLeft = left.TrimStart('0');
                var trimmedRight = right.TrimStart('0');
                var byLength = trimmedLeft.Length.CompareTo(trimmedRight.Length);
                return byLength != 0 ? byLength : string.CompareOrdinal(trimmedLeft, trimmedRight);
            }

            if (leftIsNumber != rightIsNumber)
            {
                return leftIsNumber ? -1 : 1;
            }

            return string.CompareOrdinal(left, right);
        }

        private static bool IsNumber(string part)
        {
            return part.Length > 0 && part.All(character => character >= '0' && character <= '9');
        }

        private static int Compare(int left, int right)
        {
            return left.CompareTo(right);
        }
    }
}
