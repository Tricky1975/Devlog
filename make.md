# Before you try to compile

It goes of course without saying that before you even start messing around with trying to compile this from source you need the Mono Development Kit to be installed.
Also note that the "makefile" has only be well set up for Unix based systems such as Mac and Linux, so I doubt Windows users will benefit from it.

This source code requires trickyunits for C# to be present. The best way to got for that from this folder is:
~~~shell
git clone https://github.com/tricky1975/trickyunits_csharp ../trickyunits
~~~

This code uses GTK Sharp 2, so I guess it's evident you make sure that's installed too :P

# Mascot.png

As the picture in the original binary does NOT have a free license, it has NOT been included in the github repository. However you won't be able to compile without such a picture.
Any image in png format preferably 229x314 pixels will do, and name it Mascot.png and place it in the Mascot folder, and then the compiler will take care of the rest.

# Make

When that's done all I gotta do is type "make"?
Yup! And it will compile everything... Well at least it should and with the added bonus that it creates a Mac Application bundle as well... which will only run on mac where Mono is installed though (but that goes from the release in the release section here too).

