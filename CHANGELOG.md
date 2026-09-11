# Changelog

## 2.3.0

- Add infection mode metadata, 16 Human / 10 Zombie / 5 airdrop template markers and a single Human weapon wall.
- Share safe 32-player placement and airdrop clearance geometry with the runtime.
- Validate each selected mode separately; infection does not require CT/T spawns, bombsites or barriers.
- Include the authored weapon orientation previews and exact URP light compatibility fix from the 2.2.x revisions.


## 2.2.2

- Preserve the game's URP UniversalAdditionalLightData using an exact type/assembly allowlist and a same-object Light requirement.
- Apply the same component checks before building and after AssetBundle readback; keep rejecting missing scripts and unverified components, including inactive objects.
- Include hierarchy paths in component errors; no longer describe all MonoBehaviours as missing from the game.
- Document realtime light authoring and the absence of scene lightmap export/binding.

## 2.2.1

- Replace weapon shop spheres with fixed-size weapon/attachment tiles, bounds, local axes and front arrows.
- Share WeaponShopLayout with PvPShotMode 2.2.1: authored position/rotation, local +Z front, no spawn-centre or collider offset inference.
- Add a shop marker inspector with placement/migration instructions and scale warnings.
- Place new template shops at 1.65 m with an explicit sample rotation; keep existing prefab transforms untouched.
- Keep previews editor-only; exports strip markers and preserve anchor transforms.
- Document the 2.2.1 runtime dependency and update the team deathmatch summary.

## 2.1.0

- Shared versioned MapInfo and validation contract.
- Separate marker component files, translucent Gizmos and labels.
- Mode-specific templates without environment models; real barrier colliders.
- Map definition asset with metadata, mode list and FFA settings.
- Fixed MapInfo and MapPrefab AssetBundle addresses.
- Clone-based export, marker stripping, strict validation and round-trip checks.
- Unity 6000.4.4f1 / Windows 64-bit validation.

## 1.0.0

- Initial prototype.
