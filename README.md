# OctopusEnergy_exporter

## Usage

To use this docker image, the following parameters must be provided as environment variables.

- APIKEY - This is available in your Octopus Energy account and can be found by going to Menu > My Account > Personal Details > API Access

*The following values are available on the Menu > My Account page*

![alt text](https://github.com/cdevrell/octopusenergy_exporter/blob/master/assets/variables.png?raw=true)
- ELEC_METER_MPAN
- ELEC_METER_SERIAL_NO
- ELEC_STANDING_CHARGE
- ELEC_UNIT_CHARGE
- GAS_METER_MPRN
- GAS_METER_SERIAL_NO
- GAS_STANDING_CHARGE
- GAS_UNIT_CHARGE

*These are optional values*
- RETRY_SECONDS - How often to check Octopus Energy for an update - **Defaults to 300 (5 mins)**
- PORT - Which port should the used to access the metrics - **Defaults to 9101**

~~~
docker run \
    -it \
    --rm \
    --name octopusenergy_exporter \
    -e APIKEY= \
    -e ELEC_METER_MPAN= \
    -e ELEC_METER_SERIAL_NO= \
    -e ELEC_STANDING_CHARGE= \
    -e ELEC_UNIT_CHARGE= \
    -e GAS_METER_MPRN= \
    -e GAS_METER_SERIAL_NO= \
    -e GAS_STANDING_CHARGE= \
    -e GAS_UNIT_CHARGE= \
    -p 9101:9101 \
    octopusenergy_exporter
~~~