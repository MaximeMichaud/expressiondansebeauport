.PHONY: dev

dev:
	@echo "Démarrage de la base de données (Docker)..."
	docker compose up db -d --wait
	@echo "Démarrage du backend .NET et du frontend Vite..."
	@trap 'kill 0' SIGINT; \
	(cd src/Web && dotnet run) & \
	(cd src/Web/vue-app && npm run dev) & \
	wait
