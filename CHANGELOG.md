# Changelog

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
