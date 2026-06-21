# HabitContract - 个人习惯契约打卡系统

## 项目简介

将习惯养成变成可签约、可监督、带轻惩罚的契约关系，用社交压力帮助坚持。

## 功能亮点

1. **契约签约** - 为习惯目标签订契约，设定频率和惩罚
2. **监督伙伴** - 邀请好友监督，未完成自动通知
3. **打卡记录** - 每日打卡证明，支持文字和图片
4. **违约追踪** - 自动记录违约，确认惩罚执行
5. **数据统计** - 总览统计和趋势分析

## 技术栈

- .NET 8.0 (ASP.NET Core Web API)
- MySQL 8.0 + Pomelo EF Core
- JWT Bearer Token 认证
- Swagger/OpenAPI 文档
- Docker + Docker Compose

## 目录结构

```
HabitContract/
├── src/
│   ├── HabitContract.Api/           # API 层（Controllers, Middleware）
│   ├── HabitContract.Application/   # 应用层（DTOs, Services, Mappings）
│   ├── HabitContract.Domain/        # 领域层（Entities, Interfaces, Enums）
│   └── HabitContract.Infrastructure/ # 基础设施层（DbContext, Repositories）
├── tests/
│   └── HabitContract.Tests/         # 单元测试
├── docs/
│   └── functional_intro.md          # 功能说明文档
├── docker-compose.yml
├── Dockerfile
├── .gitignore
└── HabitContract.sln
```

## 快速启动

1. 克隆项目
2. 启动服务：

```bash
docker-compose up --build -d
```

3. 访问 Swagger: http://localhost:8093/swagger
4. 健康检查: http://localhost:8093/health

## 测试

- Postman 集合导入 `postman_collection.json`
- 默认用户:
  - admin / Admin@123
  - alice / Alice@123
  - bob / Bob@123

## API 接口

| 模块 | 前缀 | 说明 |
|------|------|------|
| 认证 | /api/auth | 注册/登录/个人信息 |
| 契约 | /api/contracts | 契约 CRUD |
| 监督伙伴 | /api/contractpartners | 伙伴管理 |
| 打卡 | /api/checkins | 打卡记录 |
| 违约 | /api/contractviolations | 违约记录 |
| 统计 | /api/stats | 数据统计 |
| 健康 | /health | 健康检查 |

## 停止服务

```bash
docker-compose down -v
```
