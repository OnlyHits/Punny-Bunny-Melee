SUBMODULE_DIR = Assets/Src/Scripts/CustomArchitecture/

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
	git submodule update --remote --merge

clean:
	rm -rf $(SUBMODULE_DIR)
