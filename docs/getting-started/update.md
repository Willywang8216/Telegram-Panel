# 更新升级（Docker 部署）

> 更新前建议先备份：`./docker-data/telegram-panel.db` 与 `./docker-data/`（尤其是重要账号的 sessions）。

## 方式一：面板内一键更新（推荐先用）

入口：`左上角版本号 -> 版本信息弹窗 -> 一键更新并重启`

说明：

- 该方式基于 GitHub Release 更新包（`linux-x64/linux-arm64 zip`）
- 会自动匹配架构并部署到 `/data/app-current`
- 适合快速更新业务版本（无需手动执行命令）

## 方式二：更新 Docker 镜像（建议定期执行）

在项目目录下执行：

```bash
docker compose pull
docker compose up -d
```

适用场景：

- 更新基础镜像层（运行时/系统依赖/安全补丁）
- `.env` 的 `TP_IMAGE` 改为新 tag 后切换到指定镜像版本

## 从源码部署的用户（可选）

如果你不是用 GHCR 远程镜像，而是本地构建镜像部署，可使用：

```bash
git pull --rebase
docker compose up -d --build
```

## 更新出错：`git pull` 提示本地修改会被覆盖

典型报错：

```
error: Your local changes to the following files would be overwritten by merge:
        docker-compose.yml
Please commit your changes or stash them before you merge.
Aborting
```

原因：你本地改过 `docker-compose.yml`，导致更新时 Git 不允许直接覆盖（仅源码更新路径会遇到）。

推荐做法：尽量不要直接改 `docker-compose.yml`：

- Webhook 等部署差异：用 `.env`（参考 `.env.example`）
- 功能开关/参数：用面板「系统设置」保存到 `./docker-data/appsettings.local.json`（见 [配置与数据目录](../reference/configuration.md)）

处理方式（二选一）：

1) 放弃本地修改（最快、推荐）

```bash
git restore docker-compose.yml
git pull --rebase
docker compose up -d
```

2) 保留本地修改（自己承担后续合并成本）

```bash
git stash push -m "local docker-compose" -- docker-compose.yml
git pull --rebase
git stash pop
docker compose up -d
```

如果 `git stash pop` 出现冲突，按提示手动合并 `docker-compose.yml` 后再继续。
