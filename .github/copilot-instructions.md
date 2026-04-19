# Copilot Instructions

## Project Guidelines
- User uses Azure Key Vault secret naming convention with double hyphens to represent configuration sections (e.g., Section--Setting).
- User is strict about enforcing clean architecture dependency mapping and correct placement across architectural layers/folders.
- User wants to phase out façade services like AuthService in favor of direct MediatR (ISender) calls to Application handlers for web consumers, while mobile consumers should go through API endpoints.
- User wants strict LoggerMessage EventId governance with clear ranges by layer.
- User prefers using UnitOfWork transactional helper methods (especially ExecuteInTransactionWithRetryAsync) over manual BeginTransactionAsync/RollbackTransactionAsync patterns for resilience.
- User prefers the latest .NET LTS and latest C# language version for improved features.
- For this repository, service naming should use BaseTemplate (e.g., 'LlanCore.BaseTemplate.API') and not Onboarding.
- Follow existing CQRS convention: command and handler should be implemented in the same file, with folder naming aligned to current project conventions.

## Command Usage
- Explain command intentions before running commands/tools in future interactions.