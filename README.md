# Avatar Blendshape Utility

`R2F` is the VPM repository for **Avatar Blendshape Utility**, a Unity Editor tool for browsing, previewing, editing, saving and exporting avatar blendshape values.

## Package

- Package ID: `com.r2f.avatar-blendshape-utility`
- Display name: `Avatar Blendshape Utility`
- Unity: `2022.3`
- Menu: `R2F/Avatar Blendshape Utility`

## Local development

Open this repository as a Unity `2022.3.22f1` project. The package source is located at:

```text
Packages/com.r2f.avatar-blendshape-utility
```

You can also register that directory from VCC under `Settings > Packages > User Packages` for local testing.

## First-time GitHub setup

1. Create or push this project to a GitHub repository named `R2F`.
2. In `Settings > Secrets and variables > Actions > Variables`, create a repository variable:

   ```text
   PACKAGE_NAME=com.r2f.avatar-blendshape-utility
   ```

3. In `Settings > Pages`, set the deployment source to `GitHub Actions`.
4. Run `Actions > Build Release > Run workflow`.

The workflow creates both VPM `.zip` and legacy `.unitypackage` release files, then publishes the VCC listing at:

```text
https://r2f1007.github.io/R2F/index.json
```

## Updating

Increment the semantic version in `package.json`, push the change, then run `Build Release` again. Keep previously published releases available so existing projects can continue resolving their locked versions.

## License

Copyright © 2026 鱼. All rights reserved. See the package `LICENSE.md` file.
