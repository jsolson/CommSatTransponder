# CommSat Transponder

The purpose of this module is to allow any antenna or combination of antennas on a vessel to act as a relay.

This is an early work in progress and subject to change.

## How it works

Place a part containing a ModuleJmCommTransponder on your craft. The transponder will scan the antennas on the vessel and adopt the stronger of the strongest single active antenna, or combined power of combinable active antennas, and present itself to CommNet as a relay antenna.

Each transponder has a maximum power. Multiple transponders may be added to a vessel increase the maximum possible relaying power. The transponders have no transmission capability on their own - the power will neve exceed that of your antennas. Bandwidth as a limiting factor would seem to make more sense, but that does not appear to come into play on an unloaded vessel that is acting as a relay.

## TODO

It appears to work but needs harsher testing. I'm still waiting for the gottcha.

Parts - a test part using the small radial battery's model is included.

Balance concerns. Non relay antennas are much lighter and cheaper than relay antennas with the same range. A vessel using a Communotron-88 as relay should cost and weigh about the same as one using an RA-100.

## Adding the module to a part
~~~
MODULE
{
	name = ModuleJmCommTransponder
	maxAntennaPower = 15000000
}
~~~

## Download

https://github.com/jsolson/CommSatTransponder/releases

## Source code

https://github.com/jsolson/CommSatTransponder/

## License

This mod is distributed under the [LGPL v3.0](http://www.gnu.org/licenses/lgpl-3.0.en.html)
