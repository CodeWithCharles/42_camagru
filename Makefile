PROJECT_NAME		:= Camagru
SRC_DIR				:= src

PATH_PREFIXES		:= ${SRC_DIR}/${PROJECT_NAME}

DOMAIN_PATH			:= ${PATH_PREFIXES}.Domain
APPLICATION_PATH	:= ${PATH_PREFIXES}.Application
INFRASTRUCTURE_PATH	:= ${PATH_PREFIXES}.Infrastructure
WEB_PATH			:= ${PATH_PREFIXES}.Web

MIGRATION_DIR		:= Persistence/Migrations

.PHONY: new-migration

new-migration:
	@read -p "Enter migration name: " migration_name; \
	dotnet ef migrations add $$migration_name \
		--project src/Camagru.Infrastructure \
		--startup-project src/Camagru.Web \
		--output-dir Persistence/Migrations

down:
	docker compose down

up:
	mkdir -p uploads
	docker compose up --build