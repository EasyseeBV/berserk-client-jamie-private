using System.Text.RegularExpressions;

namespace RR.Core.Extensions
{
	public static class StringFormattingExtensions
    {
        public static string Bold(this string input)
        {
            return input.WrapWithTag("b");
        }

        public static string Italic(this string input)
        {
            return input.WrapWithTag("i");
        }

        public static string Underlined(this string input)
        {
            return input.WrapWithTag("u");
        }

        public static string CustomColor(this string input, string hexRGBA)
        {
            return input.WrapWithTag("color", hexRGBA);
        } 

        public static string Aqua(this string input)
        {
            return input.WrapWithTag("color", "#00ffffff");
        }

        public static string Black(this string input)
        {
            return input.WrapWithTag("color", "#000000ff");
        }

        public static string Blue(this string input)
        {
            return input.WrapWithTag("color", "#0000ffff");
        }

        public static string Brown(this string input)
        {
            return input.WrapWithTag("color", "#a52a2aff");
        }

        public static string Cyan(this string input)
        {
            return input.WrapWithTag("color", "#00ffffff");
        }

        public static string Darkblue(this string input)
        {
            return input.WrapWithTag("color", "#0000a0ff");
        }

        public static string Fuchsia(this string input)
        {
            return input.WrapWithTag("color", "#ff00ffff");
        }     
        
        public static string Violet(this string input)
        {
            return input.WrapWithTag("color", "#EE82EEff");
        }

        public static string Green(this string input)
        {
            return input.WrapWithTag("color", "#008000ff");
        }

        public static string DarkGreen(this string input)
        {
            return input.WrapWithTag("color", "#004000ff");
        }

		// US guys use "A"
        public static string Gray(this string input) => Grey(input);
        public static string Grey(this string input)
        {
            return input.WrapWithTag("color", "#808080ff");
        }

        public static string Lightblue(this string input)
        {
            return input.WrapWithTag("color", "#add8e6ff");
        }

        public static string Lime(this string input)
        {
            return input.WrapWithTag("color", "#00ff00ff");
        }

        public static string Magenta(this string input)
        {
            return input.WrapWithTag("color", "#ff00ffff");
        } 
        
        public static string Pink(this string input)
        {
            return input.WrapWithTag("color", "#FFC0CBff");
        }

        public static string Maroon(this string input)
        {
            return input.WrapWithTag("color", "#800000ff");
        }

        public static string Navy(this string input)
        {
            return input.WrapWithTag("color", "#000080ff");
        }

        public static string Olive(this string input)
        {
            return input.WrapWithTag("color", "#808000ff");
        }

        public static string Orange(this string input)
        {
            return input.WrapWithTag("color", "#ffa500ff");
        }

        public static string DarkOrange(this string input)
        {
            return input.WrapWithTag("color", "#ee7600ff");
        }

        public static string Purple(this string input)
        {
            return input.WrapWithTag("color", "#800080ff");
        }

        public static string Red(this string input)
        {
            return input.WrapWithTag("color", "#ff0000ff");
        }

        public static string Silver(this string input)
        {
            return input.WrapWithTag("color", "#c0c0c0ff");
        }

        public static string Teal(this string input)
        {
            return input.WrapWithTag("color", "#008080ff");
        }

        public static string White(this string input)
        {
            return input.WrapWithTag("color", "#ffffffff");
        }

        public static string Yellow(this string input)
        {
            return input.WrapWithTag("color", "#ffff00ff");
        }
        
        public static string Center(this string input)
        {
            return input.WrapWithTag("align", "center");
        }
        
        public static string Size(this string input, string size)
        {
            return input.WrapWithTag("size", size);
        }

        public static string Grad(string input, string gradName)
        {
            return input.WrapWithTag("gradient", gradName);
        }

        public static string Wrap(this string input, string wrapper)
        {
            return $"{wrapper}{input}{wrapper}";
        }

        public static string WrapWithTag(this string input, string tagName, string value = null)
        {
            return $"<{tagName}{(value != null ? $"={value}" : "")}>{input}</{tagName}>";
        }

        public static string Highlight(this string input, Regex regex, string color = "#ffff00ff")
        {
            var match = regex.Match(input);

            for (var i = 1; i < match.Groups.Count; i++)
            {
                var group = match.Groups[i];
                input = input.Replace(group.Value, group.Value.CustomColor(color));
            }

            return input;
        }

        public static string LoremIpsum(this string input, int length=30)
        {
	        return $"{input}Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur eget dapibus orci. Fusce vitae lacus interdum, lacinia sem eget, pellentesque eros. Nam arcu ipsum, pharetra id blandit eu, placerat ut leo. Sed scelerisque nisi urna, at molestie tortor ultricies a. Morbi enim tortor, tempus ut nulla nec, ultrices ultrices nibh. Nulla convallis, lectus nec semper feugiat, orci sapien mollis urna, vitae congue libero neque et metus. Donec sit amet lectus eget dui suscipit cursus. Aliquam erat volutpat.\r\n\r\nNulla sed ex ultricies, egestas purus dignissim, iaculis ante. Cras pellentesque a leo tincidunt porta. Suspendisse ultrices enim sed lectus accumsan mattis sit amet id turpis. Sed dapibus felis nec metus tempor fermentum sit amet in metus. Integer sit amet justo molestie diam laoreet molestie. Integer eros sem, blandit id nunc quis, porta tincidunt ex. Maecenas feugiat condimentum metus, nec molestie nunc interdum vitae. Sed ac libero lectus. Donec gravida dui id mauris facilisis mattis.\r\n\r\nIn ullamcorper quam et enim pulvinar dignissim. Nulla pulvinar rhoncus erat, ultrices faucibus quam faucibus eu. Mauris dapibus lobortis erat, a bibendum eros bibendum non. Etiam ut nibh vitae odio consectetur interdum vitae nec metus. Sed scelerisque diam sed venenatis suscipit. Maecenas at convallis dolor. Nam ipsum libero, ultricies eu blandit in, molestie quis purus. Mauris in diam a dui vestibulum cursus. Nulla commodo, est vitae dapibus consequat, nibh tortor euismod erat, quis dignissim lectus ante at magna. Sed tempor posuere nisi, a euismod dolor aliquet at. Nulla a nisi nulla. Donec vestibulum justo eget turpis sagittis venenatis.\r\n\r\nNam posuere quam vel neque scelerisque pretium. Proin maximus gravida mi, a aliquet nibh laoreet ac. Nam semper quis lorem vitae sagittis. Aliquam porta ligula sit amet sollicitudin viverra. Ut venenatis, dolor vel egestas iaculis, ex massa efficitur arcu, id vehicula nunc tellus at mauris. Mauris id purus non libero sagittis consequat. Curabitur tristique ac felis ut vestibulum. Cras a semper metus. Duis non nulla elit.\r\n\r\nFusce pharetra ut dui id vestibulum. Integer varius facilisis elit, non tincidunt elit sagittis eget. Vivamus vel ipsum laoreet, eleifend felis in, tincidunt est. Curabitur mollis risus nisl, in ullamcorper turpis commodo non. Phasellus interdum elementum nisi quis aliquam. Cras varius tempor dictum. Praesent nec aliquet velit, non rutrum felis. Nulla facilisi. Fusce nec turpis sollicitudin, aliquet odio eget, ornare felis."
		        .Ellipsis(length);
        }

	}
}