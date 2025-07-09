# CounterstrikeSharp - Cosmetics

[![UpdateManager Compatible](https://img.shields.io/badge/CS2-UpdateManager-darkgreen)](https://github.com/Kandru/cs2-update-manager/)
[![GitHub release](https://img.shields.io/github/release/Kandru/cs2-cosmetics?include_prereleases=&sort=semver&color=blue)](https://github.com/Kandru/cs2-cosmetics/releases/)
[![License](https://img.shields.io/badge/License-GPLv3-blue)](#license)
[![issues - cs2-map-modifier](https://img.shields.io/github/issues/Kandru/cs2-cosmetics)](https://github.com/Kandru/cs2-cosmetics/issues)
[![](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=C2AVYKGVP9TRG)

## Description

CounterstrikeSharp - Cosmetics is a plugin for Counter-Strike 2 that allows server administrators to customize the appearance of players with various cosmetic items. This plugin enhances the gaming experience by providing unique visual elements.

## Features

- Colored smoke grenades depending on the players team
- Death Beam going from the attackers eye to the impact position of the bullet


## Installation

1. Download and extract the latest release from the [GitHub releases page](https://github.com/Kandru/cs2-cosmetics/releases/).
2. Move the "Cosmetics" folder to the `/addons/counterstrikesharp/plugins/` directory.
3. Restart the server.

Updating is even easier: simply overwrite all plugin files and they will be reloaded automatically. To automate updates please use our [CS2 Update Manager](https://github.com/Kandru/cs2-update-manager/).


## Configuration

This plugin automatically creates a readable JSON configuration file. This configuration file can be found in `/addons/counterstrikesharp/configs/plugins/Cosmetics/Cosmetics.json`. Each map can have a unique configuration file. This can be achieved by creating a subfolder called `maps` next to the `Cosmetics.json`and copy the current `Cosmetics.json` as my_lowercase_map_name.json. Upon map-change this configuration file will be loaded. In case of an error within the .json file the server console will give you more information.

```json

```

TODO: configuration

## Compile Yourself

Clone the project:

```bash
git clone https://github.com/Kandru/cs2-cosmetics.git
```

Go to the project directory

```bash
  cd cs2-cosmetics
```

Install dependencies

```bash
  dotnet restore
```

Build debug files (to use on a development game server)

```bash
  dotnet build
```

Build release files (to use on a production game server)

```bash
  dotnet publish
```

## FAQ

TODO

## License

Released under [GPLv3](/LICENSE) by [@Kandru](https://github.com/Kandru).

## Authors

- [@derkalle4](https://www.github.com/derkalle4)
