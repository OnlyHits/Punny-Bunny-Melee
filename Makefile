SUBMODULE_DIR = Assets/Src/Scripts/CustomArchitecture/
SUBMODULE_URL_SSH = git@github.com:OnlyHits/CustomArchitecture.git
SUBMODULE_URL_HTTPS = https://github.com/OnlyHits/CustomArchitecture.git

.PHONY: help init update clean reset

help:
	@echo "Available targets:"
	@echo "  init     - Initialize and fetch submodules (for newcomers)"
	@echo "  update   - Update submodules to latest remote commit"
	@echo "  clean    - Remove submodule directory (if broken or corrupted)"
	@echo "  reset    - Remove and re-add the submodule from scratch"

init:
	git submodule update --init --recursive

update:
	@echo ">>> Trying SSH first..."
	@if git submodule update --remote --merge; then \
	    echo "Submodule updated using SSH"; \
	else \
	    echo ">>> SSH failed, switching to HTTPS..."; \
	    git submodule set-url $(SUBMODULE_DIR) $(SUBMODULE_URL_HTTPS); \
	    git submodule sync; \
	    git submodule update --init --remote --merge || { echo "Both SSH and HTTPS failed!"; exit 1; }; \
	fi

clean:
	rm -rf $(SUBMODULE_DIR)

switch-url:
	@echo ">>> Current URL:"; git config -f .gitmodules submodule.$(SUBMODULE_DIR).url || echo "Not set"
	@read -p ">>> Switch to (ssh/https)? " choice; \
	if [ $$choice = "ssh" ]; then \
	    git submodule set-url (SUBMODULE_DIR) $(SUBMODULE_URL_SSH); \
	elif [ $$choice = "https" ]; then \
	    git submodule set-url $(SUBMODULE_DIR) $(SUBMODULE_URL_HTTPS); \
	fi; \
	git submodule sync